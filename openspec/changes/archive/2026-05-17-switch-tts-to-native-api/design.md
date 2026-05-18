## Context

Hasta ahora `XttsTextToAudioService` usaba `OpenAIClient` del SDK OpenAI para llamar a un endpoint compatible con OpenAI (`POST /v1/audio/speech`). Esa capa no exponía el parámetro `language`, generando audio con acento inglés. El servidor XTTS ahora expone su API nativa mediante `POST /tts_to_audio/` que acepta `speaker_wav` y `language` como parámetros, permitiendo síntesis en español nativo.

## Goals / Non-Goals

**Goals:**
- Reemplazar `OpenAIClient` por `HttpClient` en `XttsTextToAudioService`
- Usar `POST /tts_to_audio/` con `speaker_wav` + `language` en vez de `POST /v1/audio/speech` con `voice`
- Añadir configuración `Xtts:Language` y renombrar `Xtts:Voice` → `Xtts:Speaker`
- Mantener la interfaz `ITextToAudioService` intacta (SK abstraction)
- El audio final se devuelve en memoria como `byte[]` (descargar el WAV del servidor y descartarlo)

**Non-Goals:**
- No se cambia `VoiceStoryService`
- No se cambia el endpoint HTTP
- No se añaden nuevas dependencias NuGet

## Decisions

### 1. HttpClient en vez de OpenAIClient

`OpenAIClient` está diseñado para la API de OpenAI. La API nativa de XTTS difiere en:
- Endpoint: `/tts_to_audio/` (no `/v1/audio/speech`)
- Body: `{ text, speaker_wav, language }` (no `{ model, input, voice }`)
- Response: `{ "url": ".../output/xxx.wav" }` (no audio directo)

`HttpClient` es más ligero, no requiere `ApiKeyCredential`, y da control total sobre la petición.

### 2. Response de la API nativa: dos llamadas

La API nativa devuelve una URL al archivo generado en vez del audio directamente:
```
POST /tts_to_audio/ → 200 { "url": "http://.../output/xxx_out.wav" }
GET /output/xxx_out.wav → 200 audio/wav (binario)
```

El flujo del servicio:
1. `POST /tts_to_audio/` con texto, speaker y language
2. Parsear JSON de respuesta para obtener `url`
3. `GET` de esa URL para descargar el WAV binario
4. Devolver `AudioContent(byte[], "audio/wav")`

### 3. Renombrado de configuración

| Antes | Después | Razón |
|-------|---------|-------|
| `Xtts:Voice` | `Xtts:Speaker` | La API nativa llama al parámetro `speaker_wav` |
| `Xtts:Model` | *(eliminado)* | El modelo lo gestiona el servidor, no el cliente |
| *(nuevo)* | `Xtts:Language` (default `es`) | La API nativa requiere `language` explícito |

### 4. Sin dependencias nuevas

`HttpClient` ya viene en elRuntime de .NET. Eliminamos la dependencia indirecta de `System.ClientModel` que traía `OpenAIClient`.

## Data Flow

```
VoiceStoryService
  → _kernel.GetRequiredService<ITextToAudioService>()
    → XttsTextToAudioService.GetAudioContentsAsync(text)
      │
      │  POST /tts_to_audio/
      │  { "text": "...", "speaker_wav": "alloy", "language": "es" }
      ▼
    XTTS server
      │
      │  { "url": "http://localhost:8020/output/abc_out.wav" }
      ▼
    GET /output/abc_out.wav
      │
      │  audio/wav (binario)
      ▼
    → AudioContent(byte[], "audio/wav")
```

## Risks / Trade-offs

- **[Risk] Dos llamadas HTTP por síntesis** → Latencia ligeramente mayor. Aceptable porque el cuello de botella es la generación del audio en GPU.
- **[Risk] El servidor acumula WAVs en `output/`** → No gestionamos la limpieza. El servidor podría llenarse de archivos. Mitigación temporal: asumir que el servidor gestiona su propio espacio. A futuro se podría añadir un `DELETE` periódico.
- **[Risk] Cambio de nombre `Xtts:Voice` → `Xtts:Speaker`** → Rompe configuraciones existentes. Se documenta en el migration guide.

## Migration Plan

1. Actualizar `appsettings.json`: añadir `Language`, renombrar `Voice` → `Speaker`, eliminar `Model`
2. Reescribir `XttsTextToAudioService.cs`
3. Actualizar `Program.cs`
4. Probar con `dotnet build` y verificar pipeline completo

## Open Questions

- *(ninguna)*
