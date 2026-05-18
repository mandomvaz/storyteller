## Why

El pipeline actual transcribe el audio y lo pasa directamente a TTS, sin generar un cuento. Se ha preparado un modelo Ollama (`storyteller`) con un system prompt incrustado que, al recibir la transcripción, genera un cuento infantil completo con el título en la primera línea. El resultado es una narración real en vez de la mera repetición de lo que el usuario dijo.

## What Changes

- **Modificado**: `VoiceStoryService.ProcessFullPipelineAsync` — añadir paso de generación de cuento vía Ollama (`storyteller`) entre Whisper y XTTS; extraer título de la línea 1 (almacenado, sin usar aún) y enviar el texto completo (título + cuerpo) a XTTS
- **Eliminado**: `VoiceStoryService.ProcessAudioAsync` — no se usa, era un esqueleto de una arquitectura por pasos
- **Modificado**: `appsettings.json` — cambiar `Ollama:TextModel` de `"llama3.2"` a `"storyteller"` (clave configurable)
- **Modificado**: `Program.cs` — default de `Ollama:TextModel` pasa a `"storyteller"`
- **Sin cambios**: API `/api/stories/new` — sigue devolviendo `Results.File(audio, "audio/wav")`

## Capabilities

### New Capabilities
- `story-generation`: Generación de cuentos a partir de texto transcrito usando Ollama, con el modelo `storyteller` (system prompt incrustado). El texto devuelto incluye el título en la primera línea.

### Modified Capabilities
- *(ninguna — los specs existentes siguen siendo válidos; el error handling del endpoint ya es genérico y cubre los nuevos casos)*

## Impact

- `storyforge/Services/VoiceStoryService.cs`: Modificar `ProcessFullPipelineAsync` para añadir paso Ollama + extracción de título; eliminar `ProcessAudioAsync`
- `storyforge/appsettings.json`: `Ollama:TextModel` → `"storyteller"`
- `storyforge/Program.cs`: Actualizar default de `Ollama:TextModel`
- Sin cambios en `XttsTextToAudioService.cs`
- Sin cambios en frontend
- Sin nuevos paquetes NuGet
