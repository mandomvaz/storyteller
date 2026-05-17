## Context

StoryForge is a .NET 10 Minimal API project with a single endpoint (`POST /api/stories/new`) that accepts audio uploads. The `VoiceStoryService` currently discards audio after validation — it's a stub returning a random GUID. There are no AI dependencies, no Ollama integration, and no Semantic Kernel packages. Two active specs exist: `audio-recorder` (frontend) and `new-story-api` (backend endpoint).

This change adds audio transcription via Faster Whisper (OpenAI-compatible API) and integrates Semantic Kernel with Ollama for future text-based AI capabilities.

## Goals / Non-Goals

**Goals:**
- Add `Microsoft.SemanticKernel` and `Microsoft.SemanticKernel.Connectors.Ollama` NuGet packages
- Register Semantic Kernel in DI connected to local Ollama (for future text features)
- Transcribe uploaded audio files to text via Faster Whisper OpenAI-compatible API
- Return `transcript` alongside `storyId` in the API response
- Configure Faster Whisper endpoint and Ollama endpoint in `appsettings.json`

**Non-Goals:**
- No frontend changes — the existing upload UI continues to work unchanged
- No story generation or LLM-powered features (future change)
- No model download management (assumes user has Faster Whisper and Ollama running)
- No Docker Compose or container orchestration
- No persistent storage of transcriptions or audio files

## Decisions

### 1. OpenAI-Compatible HTTP Client for Whisper (not Ollama)

Faster Whisper servers that expose an OpenAI-compatible API accept audio via `POST /v1/audio/transcriptions` with multipart form data (file + model name), exactly like the OpenAI API. This is a standard, well-documented protocol.

**Decision:** Create an `ITranscriptionService` interface implemented by `WhisperService`, which uses `HttpClient` to POST the audio stream (as a multipart form) to the configured Faster Whisper endpoint at `/v1/audio/transcriptions`. The response is a JSON body containing the `text` field with the transcript.

**Alternatives considered:**
- *Ollama Whisper* — requires temp-file workaround, non-standard API. Rejected: Faster Whisper's OpenAI-compatible API is simpler and more performant (streaming upload, no disk I/O).
- *SK plugin approach* — wrapping the call as a SK `ITextGenerationService`. Rejected: SK's service interface isn't designed for audio input.

### 2. Multipart Form Upload (no temp files)

The OpenAI-compatible API accepts the audio file directly in the request body as `multipart/form-data` along with a `model` field.

**Decision:** `WhisperService` reads the incoming audio stream and sends it directly as a multipart form content to the Faster Whisper endpoint. No temporary files are written. The form includes:
- `file`: the audio stream (with filename and content type)
- `model`: the configured Whisper model name (default `whisper`)

**Alternatives considered:**
- *Temp-file strategy* — writing to disk first, then reading back. Rejected: unnecessary I/O overhead when the API accepts streamed uploads.

### 3. Configuration Pattern

**Decision:** Add two configuration sections to `appsettings.json`:

`WhisperSettings`:
- `Endpoint`: The Faster Whisper server URL (default `http://localhost:8000`)
- `Model`: The model identifier (default `whisper`)
- `TimeoutSeconds`: HTTP timeout (default `60`)

`OllamaSettings`:
- `Endpoint`: The Ollama server URL (default `http://localhost:11434`)
- `TextModel`: The model name for text generation (default `llama3.2`, for future use)

Both bound via `IOptions<T>` and injected where needed.

### 4. API Response Shape

**Decision:** `POST /api/stories/new` response changes from `{ "storyId": "<guid>" }` to `{ "storyId": "<guid>", "transcript": "<transcribed text>" }`. This is a non-breaking additive change — existing frontend code parses `storyId` and ignores unknown fields.

### 5. Dependency Injection Registration

- `ITranscriptionService` → `WhisperService` registered as Scoped
- `HttpClient` via `IHttpClientFactory` (named client with configured timeout and base URL)
- Kernel via `AddKernel()` + `AddOllamaChatCompletionService` (for future text AI)
- `IOptions<WhisperSettings>` and `IOptions<OllamaSettings>` bound from configuration

### 6. Service Layer Separation

`VoiceStoryService` remains as the orchestrator — it calls `ITranscriptionService` to get the transcript, then returns the result. No transcription logic lives in the service itself, maintaining SRP.

## Risks / Trade-offs

- **[Dependency on external processes]** If Faster Whisper or Ollama is not running, transcription fails at runtime → Mitigation: catch HTTP errors and return HTTP 500 with meaningful details, configure timeouts.
- **[Audio format compatibility]** The raw webm from MediaRecorder may not be supported by Faster Whisper → Mitigation: most OpenAI-compatible whisper servers handle common audio formats; test with webm/opus and add format conversion if needed later.
- **[No SK-based transcription]** Since Faster Whisper uses OpenAI-compatible API directly (not through SK), the Kernel is only for future text features → Mitigation: this is a clean separation — SK handles text AI, a dedicated service handles audio.
