## Context

StoryForge tiene un pipeline planeado: audio → STT → story generation → TTS → imagen. Hoy solo existe el tramo STT (faster-whisper via `IAudioToTextService`). Esta PoC añade TTS (XTTS v2) para cerrar el bucle audio→texto→audio, validando que ambos servicios conviven en el mismo Kernel de SK y que el pipeline completo responde.

## Goals / Non-Goals

**Goals:**
- Integrar XTTS v2 como servicio SK (`ITextToAudioService`)
- Seguir el mismo patrón de configuración que whisper
- Pipeline: audio → STT → texto → TTS → audio/mpeg
- Respuesta binaria directa sin tocar disco
- Todo en RAM vía `byte[]`

**Non-Goals:**
- Frontend (se explorará en otro cambio)
- Story generation (Ollama todavía no participa)
- Voice cloning (se añadirá tras validar la PoC)

## Decisions

### 1. Wrapper custom `XttsTextToAudioService` en vez de `OpenAITextToAudioService`

SK provee `OpenAITextToAudioService` pero **solo acepta `apiKey` string, no `OpenAIClient`**. Como necesitamos apuntar a un endpoint local (`:8020/v1`), no podemos usar el service nativo de SK.

**Alternativas consideradas:**
| Opción | Problema |
|--------|----------|
| `AddOpenAITextToAudio(apiKey)` | Usa OpenAI público, ignora endpoint local |
| `HttpClient` con `BaseAddress` personalizado | `ClientCore` interno de SK puede ignorarlo |
| **Wrapper custom → `ITextToAudioService`** | ✅ Control total, mismo SDK, mismo patrón SK |

El wrapper es mínimo (~20 líneas), envuelve `AudioClient` del OpenAI SDK (ya instalado), y se registra en el Kernel como `ITextToAudioService`.

### 2. `AudioClient.GenerateSpeechAsync()` (sin streaming) en vez de `GenerateSpeechStreamingAsync()`

La PoC prioriza que el audio se genere completo en RAM antes de responder, evitando que un error a mitad del stream deje al cliente con audio truncado.

### 3. Misma URL base que whisper (truco del `/v1`)

XTTS v2 expone `POST /v1/audio/speech` en `:8020`. El `OpenAIClient` se construye con `Endpoint = http://localhost:8020/v1`, exactamente igual que whisper pero con otro puerto.

### 4. Misma abstracción SK

Aunque el wrapper es custom, implementa `ITextToAudioService` y se registra en el Kernel. `VoiceStoryService` (en el futuro) o el endpoint pueden resolverlo con `kernel.GetRequiredService<ITextToAudioService>()`, igual que el STT.

## Architecture

```
POST /api/stories/new (multipart, audio/*)
       │
       ▼
┌───────────────────────────────────────────────┐
│  Program.cs (endpoint)                        │
│                                               │
│  var audioService = kernel.GetRequiredService  │
│      <IAudioToTextService>();                 │
│  var text = await audioService                 │
│      .GetTextContentAsync(audioContent);       │
│                                               │
│  var ttsService = kernel.GetRequiredService    │
│      <ITextToAudioService>();                  │
│  var audioContent = await ttsService           │
│      .GetAudioContentAsync(text);             │
│                                               │
│  return Results.File(                          │
│      audioContent.Data!.ToArray(),             │
│      "audio/mpeg");                            │
└───────────────────────────────────────────────┘
       │
       ▼
  Response: audio/mpeg (completo, byte[])

Kernel
  ├── IAudioToTextService  → OpenAIAudioToTextService  (whisper :8000)
  └── ITextToAudioService  → XttsTextToAudioService    (XTTS :8020)
                              ⬆️ wrapper custom

Config
  "Xtts": {
    "Endpoint": "http://localhost:8020",
    "Model": "tts-1",
    "Voice": "default"
  }
```

## Data Flow

1. Cliente envía `multipart/form-data` con campo `audio`
2. Se valida content type (`audio/*`) y tamaño (≤10MB)
3. Se transcribe con whisper → texto
4. Se pasa el texto a XTTS v2 vía `AudioClient.GenerateSpeechAsync()`
5. Se devuelve `Results.File(byte[], "audio/mpeg")`

## Risks / Trade-offs

- **[Risk] XTTS no responde o tarda mucho** → El endpoint espera la generación completa. Si XTTS va lento (>30s), el HTTP timeout del cliente puede saltar. Mitigación: añadir un `CancellationToken` con timeout razonable en el endpoint.
- **[Risk] `OpenAITextToAudioExecutionSettings` no existe en PoC pero luego hará falta para voz** → El wrapper acepta `PromptExecutionSettings`. Al migrar a voice cloning solo cambia el valor `Voice`.
- **[Risk] El wrapper custom no sigue exactamente el `ClientCore` de SK** → Es intencional: usamos `AudioClient` directamente desde el SDK OpenAI oficial, mismo paquete que SK usa internamente.
