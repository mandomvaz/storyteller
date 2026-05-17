## Why

Completar el bucle audio→texto→audio integrando XTTS v2 como motor de TTS, siguiendo el mismo patrón que faster-whisper (OpenAI-compatible endpoint en Docker, configurable, con SK). Es una prueba de concepto que valida la segunda mitad del pipeline antes de abordar la generación de historia completa.

## What Changes

- **Nuevo**: Servicio `XttsTextToAudioService` que implementa `ITextToAudioService` usando `AudioClient` del SDK OpenAI (necesario porque SK no expone el overload con `OpenAIClient` para TTS)
- **Nuevo**: Configuración `Xtts:Endpoint`, `Xtts:Model`, `Xtts:Voice` en `appsettings.json`
- **Modificado**: Endpoint `POST /api/stories/new` — ahora recibe audio, transcribe con whisper, sintetiza con XTTS y devuelve `audio/mpeg` en vez de JSON
- **Sin cambios**: `VoiceStoryService.cs`, paquetes NuGet, Dockerfile

## Capabilities

### New Capabilities
- `tts-config`: Configuración del endpoint, modelo y voz de XTTS v2 (mismo patrón que whisper-connection)
- `tts-integration`: Servicio TTS, flujo de transcripción + síntesis, respuesta binaria streaming

### Modified Capabilities
- *(ninguna — solo se añaden capabilities nuevas)*

## Impact

- `storyforge/Program.cs`: Registrar `XttsTextToAudioService`, modificar endpoint para devolver audio
- `storyforge/Services/XttsTextToAudioService.cs`: Nuevo archivo (~20 líneas)
- `storyforge/appsettings.json`: Sección `Xtts` opcional
- Sin impacto en frontend (PoC backend-only)
- Sin nuevos paquetes NuGet (el SDK OpenAI ya está como dependencia de SK)
