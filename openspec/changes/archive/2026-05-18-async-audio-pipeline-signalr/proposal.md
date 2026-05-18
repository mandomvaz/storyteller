## Why

The audio pipeline currently blocks the HTTP request for 20-40 seconds while transcribing, generating the story, and synthesizing speech. The user gets zero feedback during this time. This is poor UX and makes the app feel broken. Decoupling the pipeline with SignalR + Channels gives real-time progress feedback, industrial-grade async processing, and teaches a production-ready communication pattern.

## What Changes

- Add SignalR hub (`/storyHub`) for real-time server→client events
- Add `BackgroundService` that reads from a `Channel<PipelineJob>` and processes the pipeline asynchronously
- Refactor `VoiceStoryService` from one monolithic method into composable steps (transcribe, generate story, generate audio)
- `POST /api/stories/new` returns `200 { jobId }` immediately after audio validation (no more 30s wait)
- Frontend connects to SignalR hub on page load, receives events: `transcribing`, `writing`, `audioReady` (binary via MessagePack)
- Frontend button states: `idle → recording → loading → playing → idle` (disabled during playback)
- Pipeline errors propagate as SignalR `error` events per step (not a single catch-all)
- Use MessagePack protocol for efficient binary audio transfer

## Capabilities

### New Capabilities
- `audio-streaming`: Real-time audio pipeline status notifications and final audio delivery via SignalR, with background processing via `Channel<T>` and `BackgroundService`

### Modified Capabilities
*(none — no existing specs are changing)*

## Impact

- **New files**: `Hubs/StoryHub.cs`, `Services/StoryPipelineBackgroundService.cs`, `Models/PipelineJob.cs`
- **Modified files**: `Program.cs` (add SignalR, Channel, BackgroundService), `VoiceStoryService.cs` (refactor into step methods), `wwwroot/index.html` (add SignalR client, new states)
- **Dependencies**: `@microsoft/signalr` (CDN), `@microsoft/signalr-protocol-msgpack` (CDN)
- **No breaking changes** to existing API shape — `POST /api/stories/new` still accepts audio, but response changes from `audio/wav` to `200 { jobId }`
