## Context

Currently, transcription works via `WhisperService` → raw `HttpClient` POST to FasterWhisper's `/v1/audio/transcriptions`. SK sits beside it, unused for transcription. The `VoiceStoryService` depends on `ITranscriptionService` (our own interface). This design replaces the ad-hoc HTTP approach with SK's native `IAudioToTextService` connector, making transcription part of the same kernel that will later handle story generation, TTS, and image generation.

## Goals / Non-Goals

**Goals:**
- Replace `WhisperService` + `ITranscriptionService` with SK's `IAudioToTextService` (OpenAI-compatible connector)
- Register the audio-to-text service in the existing Kernel alongside the Ollama chat completion
- Update `VoiceStoryService` to depend on `Kernel` instead of `ITranscriptionService`
- Remove all dead code: `WhisperService.cs`, `WhisperSettings.cs`, `ITranscriptionService.cs`, named `HttpClient`, `Whisper` config section
- Add `Microsoft.SemanticKernel.Connectors.OpenAI` NuGet dependency

**Non-Goals:**
- No change to the API contract (`POST /api/stories/new` still returns `{ storyId, transcript }`)
- No frontend changes
- No changes to FasterWhisper setup or Docker configuration
- No changes to the Ollama/SK chat completion registration
- No new capabilities — this is purely an implementation migration

## Decisions

### 1. OpenAI Audio-to-Text Connector (not a custom plugin)

FasterWhisper exposes an OpenAI-compatible API at `/v1/audio/transcriptions`. SK's `Microsoft.SemanticKernel.Connectors.OpenAI` package provides `AddOpenAIAudioToText()`, which speaks the same protocol.

**Decision:** Use `AddOpenAIAudioToText(modelId, endpoint, apiKey)` from the OpenAI connector package, pointed at the local FasterWhisper URL.

**Alternatives considered:**
- *Custom KernelPlugin wrapping HttpClient* — would work but defeats the purpose: we'd still maintain HTTP plumbing ourselves, just inside a `[KernelFunction]`. The native connector is a cleaner abstraction and teaches the real SK API.
- *Ollama multimodal* — Ollama doesn't support audio input for llama3.2. Not viable.

### 2. Same Kernel, Multiple AI Services

The existing Kernel already hosts `IChatCompletionService` (Ollama). The audio-to-text service is registered on the same Kernel builder.

**Decision:** Add `AddOpenAIAudioToText()` to the existing `builder.Services.AddKernel()` chain. Both AI services coexist in one Kernel, and `VoiceStoryService` resolves `IAudioToTextService` from it.

```
Current DI:
  Kernel ── IChatCompletionService (Ollama/llama3.2)

After change:
  Kernel ── IChatCompletionService (Ollama/llama3.2)
         ── IAudioToTextService    (FasterWhisper via OpenAI connector)
```

**Alternatives considered:**
- *Separate Kernel instances* — unnecessary overhead. A single Kernel can hold multiple AI service types.

### 3. VoiceStoryService depends on Kernel, not ITranscriptionService

The custom `ITranscriptionService` interface exists solely to abstract the Whisper HTTP call. With SK, the abstraction is `IAudioToTextService` — a standard SK interface that doesn't need our wrapper.

**Decision:** `VoiceStoryService` injects `Kernel` and resolves `IAudioToTextService` from it. The `ITranscriptionService` interface and `WhisperService` are deleted.

```csharp
// Before
public VoiceStoryService(ITranscriptionService transcriptionService)

// After
public VoiceStoryService(Kernel kernel)
```

**Alternatives considered:**
- *Keep ITranscriptionService as a thin wrapper around SK* — possible but adds zero value. If we need testability, we mock `IAudioToTextService` or the `Kernel` itself. Wrapping SK just to unwrap it later is indirection without purpose.

### 4. API Key Handling

FasterWhisper's OpenAI-compatible API typically accepts requests without a valid API key. However, SK's OpenAI connector requires an `apiKey` parameter.

**Decision:** Pass a placeholder key (`"sk-faster-whisper-local"`). If FasterWhisper rejects requests with a non-empty key, configure the connector with a custom `HttpClient` that strips the `Authorization` header.

### 5. Configuration Inline in Program.cs (match Ollama pattern)

`WhisperSettings` was needed when `WhisperService` used `IOptions<WhisperSettings>`. With SK handling the connection, the old config model is dead code.

**Decision:** Remove `WhisperSettings` class and `Whisper` config section. Read endpoint and model directly in `Program.cs` (same pattern as Ollama). Example:

```csharp
var whisperEndpoint = builder.Configuration["Whisper:Endpoint"] ?? "http://localhost:8000";
var whisperModel = builder.Configuration["Whisper:Model"] ?? "medium";
```

This keeps config available in `appsettings.json` for users who customize it, but removes the now-unnecessary POCO.

## Risks / Trade-offs

- **[SK connector compatibility]** FasterWhisper implements the OpenAI audio API but may have subtle differences (response format, headers, content-type expectations). → Mitigation: test the connector against the local FasterWhisper instance immediately after implementation.
- **[API key requirement]** SK's `AddOpenAIAudioToText` may enforce a non-empty apiKey. → Mitigation: test with a dummy key; if rejected, provide a custom `HttpClient` via `IHttpClientFactory` that omits the auth header.
- **[Loss of custom interface]** `ITranscriptionService` was a stable seam for unit-testing. Replacing it with `IAudioToTextService` changes the mocking strategy. → Mitigation: Mock `IAudioToTextService` or `Kernel` directly. This is standard practice with SK.
- **[SK version compatibility]** `IAudioToTextService` may have changed between SK 1.x releases. → Mitigation: verify the API surface of `Microsoft.SemanticKernel.Connectors.OpenAI` v1.76.0 before coding.

## Open Questions

- Does FasterWhisper accept API keys or reject requests that include one? (Needs testing with a real instance.)
- Does `AudioContent` from SK accept a raw `Stream` from the HTTP request, or does it need the full byte array? (Stream is preferable to avoid buffering the entire file in memory.)
- What is the exact method signature of `IAudioToTextService.GetTextAsync()` in SK 1.76? (Needs verification — may differ from the standard pattern.)
