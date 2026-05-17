## 1. Package Dependencies

- [x] 1.1 Add `Microsoft.SemanticKernel.Connectors.OpenAI` NuGet package to `storyforge.csproj`

## 2. Remove Dead Code

- [x] 2.1 Delete `Models/ITranscriptionService.cs`
- [x] 2.2 Delete `Services/WhisperService.cs`
- [x] 2.3 Delete `Models/WhisperSettings.cs`
- [x] 2.4 Remove `Whisper` section from `appsettings.json`
- [x] 2.5 Remove named `HttpClient` "Whisper" registration from `Program.cs`
- [x] 2.6 Remove `WhisperSettings` and `ITranscriptionService` `using`/`Configure` lines from `Program.cs`

## 3. Register SK Audio-to-Text Connector

- [x] 3.1 Read `Whisper:Endpoint` and `Whisper:Model` from configuration in `Program.cs`
- [x] 3.2 Add `AddOpenAIAudioToText()` to the existing Kernel builder chain with endpoint, model, and placeholder API key

## 4. Update VoiceStoryService

- [x] 4.1 Replace `ITranscriptionService` dependency with `Kernel`
- [x] 4.2 Resolve `IAudioToTextService` from the Kernel in `ProcessAudioAsync`
- [x] 4.3 Send audio as `AudioContent` and return the transcribed text

## 5. Verify

- [x] 5.1 Build the project and resolve any compilation errors
- [x] 5.2 Run the project and verify `POST /api/stories/new` returns transcript with valid audio
