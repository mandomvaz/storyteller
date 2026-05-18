## 1. Backend: Models

- [x] 1.1 Create `Models/PipelineJob.cs` with properties: `JobId` (Guid), `ConnectionId` (string), `AudioStream` (Stream), `ContentType` (string)

## 2. Backend: SignalR Hub

- [x] 2.1 Create `Hubs/StoryHub.cs` — empty hub class inheriting `Hub` (used only for connection management and DI)

## 3. Backend: Background Service

- [x] 3.1 Create `Services/StoryPipelineBackgroundService.cs` — `BackgroundService` that reads from `Channel<PipelineJob>` and orchestrates the pipeline
- [x] 3.2 In `ExecuteAsync`: read from channel, call transcribe → story → audio steps sequentially with individual try/catch per step
- [x] 3.3 Send SignalR events (`transcribing`, `writing`, `generatingAudio`, `audioReady`, `error`) via `IHubContext<StoryHub>` after each step

## 4. Backend: VoiceStoryService Refactor

- [x] 4.1 Extract `TranscribeAudioAsync(Stream, string)` — returns transcript string
- [x] 4.2 Extract `GenerateStoryAsync(string)` — returns story text
- [x] 4.3 Extract `GenerateAudioAsync(string)` — returns byte[] of WAV audio
- [x] 4.4 Keep `ProcessFullPipelineAsync` as a composite that calls the three steps sequentially (keeps existing usage working during transition; will be replaced by the BackgroundService)

## 5. Backend: Program.cs Wiring

- [x] 5.1 Add `builder.Services.AddSignalR()` and configure MessagePack protocol (`.AddMessagePackProtocol()`)
- [x] 5.2 Register `Channel<PipelineJob>` as singleton
- [x] 5.3 Register `StoryPipelineBackgroundService` as hosted service
- [x] 5.4 Map SignalR hub: `app.MapHub<StoryHub>("/storyHub")`
- [x] 5.5 Refactor `POST /api/stories/new` endpoint: read `connectionId` from form, validate audio, create `PipelineJob`, write to channel, return `200 { jobId }`

## 6. Frontend: SignalR Client Integration

- [x] 6.1 Add SignalR CDN scripts to `index.html` (signalr + msgpack protocol)
- [x] 6.2 Establish SignalR connection on Alpine.js `init()`, store `connectionId`
- [x] 6.3 Register event handlers: `transcribing`, `writing`, `generatingAudio`, `audioReady`, `error`
- [x] 6.4 Send `connectionId` in the FormData when uploading audio

## 7. Frontend: State Machine Update

- [x] 7.1 Update `audioRecorder()` state machine: rename `success` → `playing`, add `transcribing`/`writing`/`generatingAudio` as transient status display states
- [x] 7.2 On `audioReady`: create blob from ArrayBuffer, play audio, set state to `playing`, disable button
- [x] 7.3 Add `audio.onended` handler: reset state to `idle`, enable button
- [x] 7.4 On `error` event: display the error message with the step name, return to `idle` after a delay
- [x] 7.5 Add CSS classes for `playing` state (green, disabled style)

## 8. Cleanup

- [x] 8.1 Remove test story endpoint `/api/stories/test` and its frontend button
- [x] 8.2 Remove unused imports and dead code from refactored files
- [x] 8.3 Verify the app builds and runs without errors
