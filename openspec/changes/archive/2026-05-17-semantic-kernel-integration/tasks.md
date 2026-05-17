## 1. Package Dependencies

- [x] 1.1 Add `Microsoft.SemanticKernel` NuGet package to `storyforge.csproj`
- [x] 1.2 Add `Microsoft.SemanticKernel.Connectors.Ollama` NuGet package to `storyforge.csproj`

## 2. Configuration

- [x] 2.1 Create `WhisperSettings` class with `Endpoint`, `Model`, and `TimeoutSeconds` properties
- [x] 2.2 Create `OllamaSettings` class with `Endpoint` and `TextModel` properties
- [x] 2.3 Add `Whisper` and `Ollama` sections to `appsettings.json` with default values
- [x] 2.4 Register both settings classes via `IOptions<T>` in DI

## 3. Transcription Service

- [x] 3.1 Create `ITranscriptionService` interface with `TranscribeAsync(Stream audioStream, string contentType)` method
- [x] 3.2 Implement `WhisperService` that sends audio as multipart form data to the configured Whisper endpoint (`/v1/audio/transcriptions`), reads `text` from the JSON response
- [x] 3.3 Register `ITranscriptionService` → `WhisperService` as Scoped in DI with a named `HttpClient`

## 4. Semantic Kernel Integration

- [x] 4.1 Register Kernel in DI via `AddKernel()` with Ollama chat completion service pointing to the configured `Ollama:Endpoint` and `Ollama:TextModel`

## 5. VoiceStoryService Update

- [x] 5.1 Update `VoiceStoryService` to inject `ITranscriptionService` and call `TranscribeAsync` in `ProcessAudioAsync`
- [x] 5.2 Update return type to include transcribed text alongside storyId

## 6. API Endpoint Update

- [x] 6.1 Update `POST /api/stories/new` handler to return `{ storyId, transcript }` instead of just `{ storyId }`
- [x] 6.2 Add error handling for transcription failures (HTTP 500 with meaningful message)

## 7. Verify

- [x] 7.1 Build the project and resolve any compilation errors
- [x] 7.2 Run the project and verify `POST /api/stories/new` returns transcript with valid audio
