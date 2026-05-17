## Why

StoryForge captures audio from users but currently does nothing with it — the `VoiceStoryService` is a stub that discards the audio after validation. Integrating a transcription pipeline will unlock voice-to-text capabilities, while Semantic Kernel + Ollama lays the groundwork for future AI-powered features (story generation, summarization). Using Faster Whisper (OpenAI-compatible API) for transcription keeps the integration standard, fast, and locally private.

## What Changes

- Replace the stub `VoiceStoryService.ProcessAudioAsync` with real transcription via Faster Whisper (OpenAI-compatible API)
- Add `Microsoft.SemanticKernel` and `Microsoft.SemanticKernel.Connectors.Ollama` NuGet packages
- Add configuration for Faster Whisper endpoint URL and model
- Add configuration for Ollama endpoint URL and text model (for future SK features)
- Register Semantic Kernel in DI with Ollama connector for text models
- Transcribe uploaded audio to text by calling the OpenAI-compatible `/v1/audio/transcriptions` endpoint
- Return the transcribed text alongside `storyId` in the API response
- Keep all existing validation, frontend, and API contract intact

## Capabilities

### New Capabilities
- `ollama-connection`: Configuration, connection, and health-checking for a local Ollama server instance (used by Semantic Kernel for text models)
- `whisper-connection`: Configuration and connection to the Faster Whisper server exposing an OpenAI-compatible API
- `audio-transcription`: Transcribe uploaded audio files to text via Faster Whisper (OpenAI-compatible `/v1/audio/transcriptions` endpoint)

### Modified Capabilities
- `new-story-api`: The `POST /api/stories/new` response will now include a `transcript` field with the transcribed text (non-breaking addition)

## Impact

- **Dependencies**: `Microsoft.SemanticKernel` + `Microsoft.SemanticKernel.Connectors.Ollama` NuGet packages added to `storyforge.csproj`
- **Configuration**: `appsettings.json` extended with `Whisper` section (endpoint, model) and `Ollama` section (endpoint, text model)
- **Services**: `VoiceStoryService` gains `ITranscriptionService` dependency; `ProcessAudioAsync` returns transcribed text
- **API Response**: `POST /api/stories/new` response changes from `{ "storyId" }` to `{ "storyId", "transcript" }` (non-breaking — `transcript` is additive)
- **Frontend**: No changes required — the existing upload flow continues to work; transcript handling can be added later
- **Docker**: No immediate changes needed; Faster Whisper and Ollama run separately on the host
