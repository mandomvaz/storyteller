## Context

The current pipeline processes audio → transcription → story → TTS sequentially as a single atomic unit. `VoiceStoryService` calls all three SK services in sequence and the frontend gets a single `"audioReady"` event when everything is done. This design refactors the TTS and delivery stages into independent scoped services connected by `Channel<T>`, enabling per-paragraph parallel processing.

Existing architecture:
```
PipelineJob (singleton channel)
  → StoryPipelineBackgroundService
    → VoiceStoryService (scoped via factory)
      1. IAudioToTextService (Whisper)
      2. IChatCompletionService (Ollama, full text)
      3. ITextToAudioService (XTTS, full text)
    → SignalR: "audioReady" (single blob)
```

Target architecture:
```
PipelineJob (singleton channel)
  → StoryPipelineWorker (singleton BackgroundService)
    → IServiceScopeFactory → scope
      → StoryPipelineRunner (scoped)
        ┌─────────────────────────────────────────────────┐
        │  task 1: VoiceStoryService                       │
        │    → Whisper (SK)                                │
        │    → Ollama (SK) → parsea \n\n                   │
        │    → escribe TextUnit[] en Channel<TextUnit>     │
        │    → Complete() cuando se acaban los párrafos    │
        │                                                  │
        │  task 2: TextToAudioService                      │
        │    → Lee Channel<TextUnit>                       │
        │    → ITextToAudioService (SK → XTTS)             │
        │    → escribe AudioUnit[] en Channel<AudioUnit>   │
        │    → Complete() cuando se acaba textChannel       │
        │                                                  │
        │  task 3: AudioDeliveryService                    │
        │    → Lee Channel<AudioUnit>                       │
        │    → SignalR: "audioChunk" (por cada unidad)      │
        │    → SignalR: "storyComplete" (cuando Complete)   │
        └─────────────────────────────────────────────────┘
```

## Goals / Non-Goals

**Goals:**
- Desacoplar TTS y delivery de `VoiceStoryService` en servicios scoped independientes
- Procesar TTS por párrafo a medida que Ollama los produce, no cuando termina todo el cuento
- Entregar audio al browser en chunks en lugar de un solo blob
- Mantener fidelidad a SK: toda interacción con modelos de IA va por SK
- Los servicios nuevos y refactorizados siguen SRP

**Non-Goals:**
- No se toca el frontend (salvo el nombre del evento SignalR)
- No se cambia el modelo de datos (`PipelineJob`, `Story`, `OllamaSettings`)
- No se añade persistencia ni historial
- No se implementa cancelación de jobs
- No se gestionan múltiples consumidores por channel (single reader)

## Decisions

### Channels scoped vs singleton
| Opción | Veredicto |
|---|---|
| Canales singleton con `.Complete()` | ❌ Un `.Complete()` mata el canal para todos los jobs futuros |
| Canales creados a mano en `StoryPipelineWorker` | ❌ El worker no debería saber de canales, viola SRP |
| **Canales scoped** | ✅ El contenedor crea un par por scope, los inyecta a todos los servicios, se destruyen al salir del scope. `.Complete()` funciona porque cada job tiene su par |

### Un solo consumidor por canal
`SingleReader = true` en ambos canales. `TextToAudioService` es el único lector de `Channel<TextUnit>`, `AudioDeliveryService` el único de `Channel<AudioUnit>`. Esto garantiza orden FIFO sin necesidad de índices ni números de secuencia.

### Señal de finalización: `.Complete()` descendente
`VoiceStoryService` llama `textChannel.Writer.Complete()` cuando termina de emitir todos los párrafos. `TextToAudioService` ve el cierre, drena los `TextUnit` pendientes, y llama `audioChannel.Writer.Complete()`. `AudioDeliveryService` ve el cierre y envía `"storyComplete"`. No hay mensajes centinela, no hay `IsFinal`, no hay gestión manual de estado.

### Errores: cualquiera puede notificar
Todos los servicios scoped reciben `IHubContext<StoryHub>` en el constructor. Si cualquier paso falla, ese servicio envía `"error"` con `{ jobId, connectionId, step, message }` directamente al browser. `StoryPipelineRunner` detecta la task fallida con `Task.WhenAny` y deja que las otras tasks se cancelen vía `CancellationToken` — pero en la práctica los channels se drenan y mueren solos al salir del scope.

### Texto plano como entrada de TTS, sin distinción título/cuerpo
`TextUnit` contiene solo `JobId`, `ConnectionId`, `Text`. El primer párrafo es el título, el resto son párrafos del cuerpo. `TextToAudioService` trata todos igual. El browser reproduce los chunks en orden sin necesidad de saber si es título o párrafo.

## Risks / Trade-offs

| Riesgo | Mitigación |
|---|---|
| Si `TextToAudioService` falla, `AudioDeliveryService` espera indefinidamente en el channel | Se inyecta `CancellationToken` a todos los servicios. Si una task falla, `StoryPipelineRunner` cancela el token y las otras tasks se desbloquean |
| XTTS puede producir audio a distinta velocidad que Ollama produce texto | Channels unbounded absorben la diferencia. Si XTTS va más lento, los `TextUnit` se acumulan en RAM — máximo ~10 párrafos, riesgo insignificante |
| El orden de llegada al browser es el orden de entrada al canal (FIFO), pero si el browser reproduce en el orden de `"audioChunk"` no hay problema | Depende de que el browser encole y reproduzca secuencialmente. Asumimos que el frontend actual encola en orden de recepción |
| El `"storyComplete"` llega antes que el último `"audioChunk"` si hay race condition | No es posible si los eventos se envían en orden en `AudioDeliveryService` — `"storyComplete"` se envía después del último `"audioChunk"`, cuando `ReadAllAsync` retorna, y SignalR garantiza orden en una misma conexión |
