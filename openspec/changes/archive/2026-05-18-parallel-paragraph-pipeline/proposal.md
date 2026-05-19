## Why

The current pipeline processes the entire story as one atomic unit: transcribe → generate full story text → generate full audio → deliver. The frontend gets zero feedback until every paragraph is written AND synthesized. By switching to per-paragraph processing, audio generation and delivery start while Ollama is still generating subsequent paragraphs, dramatically reducing time-to-first-audio and providing progressive delivery to the browser.

## What Changes

- **BREAKING**: `VoiceStoryService` no longer calls TTS directly — it writes `TextUnit` chunks to a `Channel<TextUnit>` instead
- Add `TextToAudioService` (scoped): reads `Channel<TextUnit>`, calls XTTS via SK `ITextToAudioService`, writes `AudioUnit` to `Channel<AudioUnit>`
- Add `AudioDeliveryService` (scoped): reads `Channel<AudioUnit>`, sends `"audioChunk"` events via SignalR to the browser
- Add `StoryPipelineRunner` (scoped): lanza las 3 tasks concurrentes (`VoiceStoryService`, `TextToAudioService`, `AudioDeliveryService`)
- Refactor `StoryPipelineBackgroundService` → `StoryPipelineWorker`: singleton, crea scopes, delega en `StoryPipelineRunner`
- Refactor `VoiceStoryService.ProcessAsync`: ahora escribe `TextUnit`s al channel a medida que Ollama produce párrafos (separados por `\n\n`), y llama `Complete()` cuando termina
- **BREAKING**: Evento `"generatingAudio"` eliminado, reemplazado por múltiples `"audioChunk"` + `"storyComplete"`
- `Channel<PipelineJob>` sigue igual (singleton)
- `Channel<TextUnit>` y `Channel<AudioUnit>` nuevos, registrados como scoped
- Registros DI: `TextToAudioService`, `AudioDeliveryService`, `StoryPipelineRunner` como scoped

## Capabilities

### New Capabilities
- `per-paragraph-tts`: Per-paragraph text-to-audio processing via dedicated `TextToAudioService` consuming from `Channel<TextUnit>`, with `AudioDeliveryService` sending individual chunks to the browser via SignalR

### Modified Capabilities
- `audio-streaming`: Los eventos cambian. `"generatingAudio"` + `"audioReady"` se reemplazan por `"audioChunk"` (por cada unidad de audio lista) + `"storyComplete"` (cuando termina todo el pipeline). El flujo de comunicación cambia de un solo blob binario a chunks secuenciales.
- `story-generation`: La generación de historia ahora produce texto en streaming. Los párrafos se emiten uno a uno al `Channel<TextUnit>` a medida que se detectan por `\n\n`, y el título se trata como el primer párrafo. `VoiceStoryService` ya no retorna el texto completo, sino que lo escribe en un canal.
- `tts-integration`: El TTS ya no se llama directamente desde `VoiceStoryService`. Pasa a ser un servicio scoped independiente (`TextToAudioService`) que consume unidades de texto desde un channel y produce unidades de audio en otro channel. El `XttsTextToAudioService` (wrapper de SK `ITextToAudioService`) se mantiene igual.

## Impact

- **New files**: `Services/TextToAudioService.cs`, `Services/AudioDeliveryService.cs`, `Services/StoryPipelineRunner.cs`
- **Renamed files**: `Services/StoryPipelineBackgroundService.cs` → `Services/StoryPipelineWorker.cs`
- **Modified files**: `Program.cs` (nuevos registros DI, renombrado), `Services/VoiceStoryService.cs` (refactor a channel writer), `Hubs/StoryHub.cs` (sin cambios)
- **Removed**: evento `"generatingAudio"` del protocolo SignalR (no es breaking para frontend si se actualiza)
- **Dependencies**: ninguna nueva
