## Why

Durante el desarrollo, hay que hablar al micrófono cada vez que se prueba el pipeline. Se necesita un botón que dispare un ejemplo con texto fijo para iterar rápido, y de paso sirve de banco de pruebas para la paralelización del pipeline (streaming de Ollama → envío por párrafos a XTTS).

## What Changes

- **Nuevo**: Modelo `Story` con `Title`, `Badge` (string de emojis, para futuro), `List<string> Paragraphs`
- **Nuevo**: Endpoint `POST /api/stories/test` que recibe texto fijo, salta Whisper, y ejecuta el pipeline desde Ollama en adelante
- **Modificado**: `VoiceStoryService` — nuevo método que recibe texto directamente y usa streaming de Ollama para rellenar el objeto `Story` por párrafos, luego envía título + párrafos a XTTS
- **Nuevo**: Botón en frontend que llama a `/api/stories/test`
- **Sin cambios**: Endpoint `/api/stories/new` existente (sigue igual)
- **Temporal**: Este endpoint y botón se eliminarán cuando se integre la feature real

## Capabilities

### New Capabilities
- *(ninguna — es una herramienta de desarrollo, no una capability nueva)*

### Modified Capabilities
- *(ninguna — no cambian requisitos existentes)*

## Impact

- `storyforge/Models/Story.cs`: Nuevo modelo
- `storyforge/Services/VoiceStoryService.cs`: Nuevo método con streaming vía `IChatCompletionService.GetStreamingChatMessageContentsAsync`
- `storyforge/Program.cs`: Nuevo endpoint `/api/stories/test`
- `storyforge/wwwroot/index.html`: Botón de prueba + lógica Alpine.js
