## Context

El cambio `2026-05-17-xtts-text-to-speech` implementó `XttsTextToAudioService` como wrapper custom de `ITextToAudioService`, pero lo registró en `builder.Services` (DI estándar de ASP.NET) en vez de en el `IKernelBuilder` que devuelve `AddKernel()`. Esto creó una inconsistencia:

| Servicio | Registrado en | Resuelto via |
|----------|--------------|-------------|
| `IAudioToTextService` (STT) | `IKernelBuilder` (AddOpenAIAudioToText) | `kernel.GetRequiredService<>()` |
| `ITextToAudioService` (TTS) | `builder.Services` (AddSingleton directo) | Inyección directa en el endpoint |

Además, `VoiceStoryService` solo orquesta STT, y el endpoint orquesta TTS directamente — el pipeline está partido en dos lugares.

## Goals / Non-Goals

**Goals:**
- Registrar `ITextToAudioService` en el mismo flujo del Kernel builder que `IAudioToTextService`
- Centralizar la orquestación del pipeline audio→texto→audio en `VoiceStoryService`
- El endpoint solo llama a `VoiceStoryService` para el pipeline completo
- Cero cambios en `XttsTextToAudioService.cs` (su implementación es correcta)

**Non-Goals:**
- No se añade generación de historia (Ollama) — eso vendrá en otro cambio
- No se cambia la configuración ni los specs existentes
- No se modifica el frontend

## Decisions

### 1. Registrar TTS en el Kernel builder, no en builder.Services

El `IKernelBuilder` que devuelve `AddKernel()` tiene su propio `IServiceCollection`. Aunque en SK moderno ambos collections se fusionan, el `Kernel` internamente resuelve servicios desde su propio scope. Registrar TTS en el Kernel builder garantiza que `kernel.GetRequiredService<ITextToAudioService>()` funcione, exactamente igual que STT.

```csharp
// Antes (roto):
builder.Services.AddKernel()
    .AddOllamaChatCompletion(ollamaModel, new Uri(ollamaEndpoint))
    .AddOpenAIAudioToText(whisperModel, whisperClient);

builder.Services.AddSingleton<ITextToAudioService>(sp =>
    new XttsTextToAudioService(xttsClient, xttsModel, xttsVoice));

// Después (correcto):
var kernelBuilder = builder.Services.AddKernel()
    .AddOllamaChatCompletion(ollamaModel, new Uri(ollamaEndpoint))
    .AddOpenAIAudioToText(whisperModel, whisperClient);

kernelBuilder.Services.AddSingleton<ITextToAudioService>(sp =>
    new XttsTextToAudioService(xttsClient, xttsModel, xttsVoice));
```

### 2. VoiceStoryService orquesta el pipeline completo

`VoiceStoryService` ya recibe `Kernel` via constructor DI. Se le añade un método `ProcessFullPipelineAsync()` que:
1. Toma el stream de audio + contentType
2. Resuelve `IAudioToTextService` del Kernel → transcribe
3. Resuelve `ITextToAudioService` del Kernel → sintetiza
4. Devuelve `(string Transcript, byte[] AudioData)`

### 3. El endpoint delega en VoiceStoryService

El endpoint `POST /api/stories/new` solo inyecta `VoiceStoryService` y `CancellationToken`. Llama al pipeline completo y devuelve `Results.File()`.

Esto mantiene SRP: el endpoint maneja HTTP, el servicio maneja la lógica de negocio.

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                     Program.cs                           │
│                                                          │
│  builder.Services.AddKernel()                            │
│    .AddOllamaChatCompletion(...)                          │
│    .AddOpenAIAudioToText(whisper...)                     │
│    .Services.AddSingleton<ITextToAudioService>(...)       │
│                                             ← TTS ahora  │
│                                                          │
│  MapPost("/api/stories/new")                             │
│    └── VoiceStoryService.ProcessFullPipelineAsync()      │
│          ├── _kernel.GetService<IAudioToTextService>()   │
│          ├── _kernel.GetService<ITextToAudioService>()   │
│          └── return (transcript, audioBytes)             │
│                                                          │
│    → Results.File(audioBytes, "audio/wav")               │
└──────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────┐      ┌──────────────────────┐
│  IAudioToTextService │      │  ITextToAudioService │
│  (whisper :8000)     │      │  (XTTS :8020)        │
│  en Kernel           │      │  en Kernel           │
└──────────────────────┘      └──────────────────────┘
```

## Risks / Trade-offs

- **[Risk] `kernelBuilder.Services` vs `builder.Services` podrían ser el mismo collection** → En algunas versiones de SK, `IKernelBuilder.Services` es el mismo `IServiceCollection` que `builder.Services`. La asignación a una variable `kernelBuilder` es semántica, no funcional. El beneficio real es la claridad de intención y que ambos servicios aparezcan en el mismo bloque de código.
- **[Risk] VoiceStoryService crece en responsabilidad** → Al añadir TTS, el servicio maneja ambos extremos del pipeline. Si luego se añade generación de historia, podría tocar dividirlo (`AudioPipelineService`). Se acepta como trade-off por ahora (sigue siendo SRP: "orquestar el pipeline de audio").

## Open Questions

- *(ninguna — la solución está clara)*
