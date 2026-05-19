## 1. DI Registration & Renames

- [x] 1.1 Rename `StoryPipelineBackgroundService` → `StoryPipelineWorker` (class + file + `Program.cs` registration)
- [x] 1.2 Register `Channel<TextUnit>` as scoped (SingleReader = true) in `Program.cs`
- [x] 1.3 Register `Channel<AudioUnit>` as scoped (SingleReader = true) in `Program.cs`
- [x] 1.4 Register `StoryPipelineRunner` as scoped in `Program.cs`
- [x] 1.5 Register `TextToAudioService` as scoped in `Program.cs`
- [x] 1.6 Register `AudioDeliveryService` as scoped in `Program.cs`
- [x] 1.7 Register `VoiceStoryService` continues as scoped (already registered)

## 2. Data Models

- [x] 2.1 Create `TextUnit` record: `string JobId`, `string ConnectionId`, `string Text`
- [x] 2.2 Create `AudioUnit` record: `string JobId`, `string ConnectionId`, `byte[] AudioData`

## 3. TextToAudioService (new)

- [x] 3.1 Create `TextToAudioService` implementing `RunAsync(CancellationToken)`
- [x] 3.2 Inject `Channel<TextUnit>` (reader), `Channel<AudioUnit>` (writer), `Kernel`, `IHubContext<StoryHub>`
- [x] 3.3 Loop on `_textCh.Reader.ReadAllAsync(ct)`, calling `ITextToAudioService.GetAudioContentAsync()` for each `TextUnit`
- [x] 3.4 Write resulting `AudioUnit` to `_audioCh.Writer`
- [x] 3.5 Call `_audioCh.Writer.Complete()` when `ReadAllAsync` completes (textChannel closed)
- [x] 3.6 Catch errors and send `"error"` SignalR event with `{ jobId, connectionId, step: "tts", message }`

## 4. AudioDeliveryService (new)

- [x] 4.1 Create `AudioDeliveryService` implementing `RunAsync(CancellationToken)`
- [x] 4.2 Inject `Channel<AudioUnit>` (reader), `IHubContext<StoryHub>`
- [x] 4.3 Loop on `_audioCh.Reader.ReadAllAsync(ct)`, sending `"audioChunk"` SignalR event with raw binary audio for each `AudioUnit`
- [x] 4.4 Send `"storyComplete"` SignalR event when `ReadAllAsync` completes (audioChannel closed)
- [x] 4.5 Catch errors and send `"error"` SignalR event with `{ jobId, connectionId, step: "delivery", message }`

## 5. VoiceStoryService (refactor)

- [x] 5.1 Add `Channel<TextUnit>` and `IHubContext<StoryHub>` as constructor dependencies
- [x] 5.2 Refactor `GenerateStoryFromTextAsync` → remove TTS call at the end, instead write paragraphs one by one to `Channel<TextUnit>` as streaming detects `\n\n`
- [x] 5.3 The title IS the first paragraph written to the channel (no special treatment)
- [x] 5.4 Call `_textCh.Writer.Complete()` when the Ollama stream ends and all paragraphs are written
- [x] 5.5 Remove `GenerateAudioAsync` and `ProcessFullPipelineAsync` methods (TTS is now `TextToAudioService`'s job)
- [x] 5.6 Catch errors and send `"error"` SignalR event with `{ jobId, connectionId, step: "story"|"transcription", message }`

## 6. StoryPipelineRunner (new)

- [x] 6.1 Create `StoryPipelineRunner` injecting `VoiceStoryService`, `TextToAudioService`, `AudioDeliveryService`
- [x] 6.2 `RunAsync(PipelineJob, CancellationToken)` — lanza las 3 tasks concurrentes
- [x] 6.3 Usar `Task.WhenAny` para detectar fallos y propagar cancelación
- [x] 6.4 No gestiona channels directamente (todo inyectado por DI)

## 7. StoryPipelineWorker (rename + refactor)

- [x] 7.1 Rename class to `StoryPipelineWorker` (file rename too)
- [x] 7.2 Remove `IHubContext<StoryHub>` dependency (ya no envía eventos directamente)
- [x] 7.3 Loop en `Channel<PipelineJob>` → crea scope → resuelve `StoryPipelineRunner` → llama `RunAsync`

## 8. Frontend (minor)

- [x] 8.1 Replace `"generatingAudio"` listener with nothing (ese estado ya no existe)
- [x] 8.2 Replace `"audioReady"` listener with `"audioChunk"` handler that appends audio data to a playback queue
- [x] 8.3 Add `"storyComplete"` listener (opcional, para UI feedback)
