## Why

Semantic Kernel is already registered in DI but sits unused — the transcription pipeline bypasses it with a raw `HttpClient` call to FasterWhisper. Since the project's primary value is learning SK, every AI capability should flow through its native abstractions. This change replaces the manual HTTP approach with SK's `IAudioToTextService` connector, making transcription a first-class citizen in the SK kernel alongside future pipeline stages.

## What Changes

- Replace `WhisperService` (raw `HttpClient` + `MultipartFormDataContent`) with SK's `IAudioToTextService` connector pointed at the local FasterWhisper endpoint
- Replace `ITranscriptionService` interface usage with direct SK kernel calls
- Remove named `HttpClient` registration for "Whisper" from DI
- Remove `WhisperSettings` configuration model and its `appsettings.json` section (endpoint moves to the SK connector config)
- Update `VoiceStoryService` to inject `Kernel` instead of `ITranscriptionService`
- Update `Program.cs` to register the SK audio-to-text connector

## Capabilities

### New Capabilities

- *(none — all capabilities already exist, this changes implementation only)*

### Modified Capabilities

- `audio-transcription`: The "how" changes from raw HTTP multipart to SK's native `IAudioToTextService`. Spec-level requirements shift from implementation details (multipart form, file field) to SK connector semantics (audio content stream, connector configuration).
- `whisper-connection`: The connection model changes — instead of a `WhisperSettings` POCO bound via `IOptions`, the endpoint is configured through the SK connector registration.

## Impact

- **Dependencies**: Likely needs `Microsoft.SemanticKernel.Connectors.OpenAI` NuGet package (for `AddOpenAIAudioToText` extension)
- **Removals**: `WhisperService.cs`, `WhisperSettings.cs`, `ITranscriptionService.cs`, named `HttpClient` "Whisper" registration, `Whisper` section from `appsettings.json`
- **Modified**: `VoiceStoryService.cs` (dependency from `ITranscriptionService` → `Kernel`), `Program.cs` (DI registration)
- **API Response**: No change — `{ storyId, transcript }` shape stays identical
- **Frontend**: No change
- **FasterWhisper**: Still runs locally in Docker; the SK connector speaks OpenAI-compatible API which FasterWhisper already exposes
