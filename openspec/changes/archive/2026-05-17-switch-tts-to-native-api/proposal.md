## Why

El servidor XTTS v2 ha cambiado de una capa de compatibilidad OpenAI a su API nativa. La capa OpenAI-compatible no exponía el parámetro `language`, por lo que el TTS generaba audio con acento inglés en vez de español nativo. La API nativa sí soporta `language`, dando audio en español sin acento extranjero.

## What Changes

- **Modificado**: `Services/XttsTextToAudioService.cs` — reescritura completa: usar `HttpClient` en vez de `OpenAIClient`, llamar a `POST /tts_to_audio/` con `speaker_wav` y `language`, descargar el audio de la URL devuelta
- **Modificado**: `Program.cs` — eliminar creación de `OpenAIClient` para XTTS, registrar `HttpClient` + nuevos parámetros de configuración
- **Modificado**: `appsettings.json` — añadir `Xtts:Language`, eliminar `Xtts:Model`
- **Modificado**: `openspec/specs/tts-config/spec.md` — actualizar requisitos para reflejar la API nativa (endpoint sin `/v1`, `speaker_wav` en vez de `voice`, añadir `language`)
- **Modificado**: `openspec/specs/tts-integration/spec.md` — actualizar escenarios de error para reflejar `HttpClient` en vez de `AudioClient`

## Capabilities

### New Capabilities
- *(ninguna — solo se modifican las existentes)*

### Modified Capabilities
- `tts-config`: Cambia el mecanismo de conexión (`OpenAIClient` → `HttpClient`), la configuración de voz (`Xtts:Voice` → `Xtts:Speaker`), se añade `Xtts:Language`, se elimina `Xtts:Model`
- `tts-integration`: Escenarios de error actualizados para reflejar la nueva implementación HTTP

## Impact

- `storyforge/Services/XttsTextToAudioService.cs`: Reescribir (misma interfaz SK, distinto backend)
- `storyforge/Program.cs`: Eliminar `OpenAIClient` + `ApiKeyCredential` para XTTS, añadir `HttpClient`
- `storyforge/appsettings.json`: `Xtts:Language` → `"es"`, `Xtts:Speaker` → `"alloy"`, eliminar `Xtts:Model`
- Sin cambios en `VoiceStoryService.cs`
- Sin cambios en frontend
- Sin nuevos paquetes NuGet (eliminamos dependencia de `System.ClientModel`)
