## Context

Actualmente `VoiceStoryService.ProcessFullPipelineAsync` transcribe el audio vía Whisper y pasa el texto directamente a XTTS, sin generación de cuento. El Kernel ya tiene registrado `AddOllamaChatCompletion` con endpoint y modelo configurables. Se ha preparado un modelo Ollama (`storyteller`) con un system prompt incrustado para generar cuentos infantiles a partir de la transcripción.

El nuevo flujo inserta un paso de inferencia con `IChatCompletionService` entre Whisper y XTTS. El modelo devuelve el cuento completo con el título en la primera línea. El título se extrae (almacenado, sin usar aún) y el texto completo (título + cuerpo) se envía a XTTS para locución.

## Goals / Non-Goals

**Goals:**
- Insertar generación de cuento vía Ollama entre Whisper y XTTS en `ProcessFullPipelineAsync`
- Extraer la primera línea del texto generado como título del cuento
- Enviar el texto completo (incluyendo título) a XTTS para locución
- Almacenar el título como variable aunque no se use aún
- Eliminar `ProcessAudioAsync` (código muerto)
- Mantener el endpoint `/api/stories/new` inalterado (sigue devolviendo `audio/wav`)
- Usar `IChatCompletionService` de Semantic Kernel (no HTTP directo)
- Modelo configurable vía `Ollama:TextModel` (default `storyteller`)

**Non-Goals:**
- No se expone el título al frontend (se almacena internamente)
- No se cambia el contrato de la API
- No se añaden nuevas dependencias NuGet
- No se modifica `XttsTextToAudioService`
- No se modifican specs existentes

## Decisions

### 1. Usar IChatCompletionService de SK

En vez de llamar a Ollama por HTTP directo, se usa `kernel.GetRequiredService<IChatCompletionService>()`. El Kernel ya tiene registrado `AddOllamaChatCompletion`, que proporciona esta interfaz. SK gestiona la conexión, serialización y reintentos.

Alternativa descartada: HTTP directo a `POST /api/chat`. Más simple pero rompe el principio del proyecto de pasar por las abstracciones de SK.

### 2. Sin prompt adicional: el texto transcrito se envía directamente

El modelo `storyteller` ya tiene el system prompt incrustado. La llamada a `IChatCompletionService.GetChatMessageContentAsync` recibe el transcript como único mensaje de usuario.

### 3. Título extraído por split en primera línea

```csharp
var lines = storyText.Split('\n', StringSplitOptions.None);
var title = lines.FirstOrDefault()?.Trim() ?? string.Empty;
var fullText = storyText;  // se envía completo a XTTS
```

Si el modelo devuelve una sola línea (caso borde improbable), el título y el cuerpo son el mismo texto — XTTS locuta el título únicamente. Aceptable como comportamiento por defecto.

### 4. El título se almacena pero no se usa

`ProcessFullPipelineAsync` cambia su firma para devolver `(string Transcript, byte[] AudioData, string Title)`. El endpoint recibe el título pero no lo incluye en la respuesta. El GC lo recolecta. Allana el terreno para usos futuros.

## Risks / Trade-offs

- **[Risk] El modelo storyteller devuelve formato inesperado** → El split por `\n` es tolerante: si no hay `\n`, `title = storyText`. Si hay múltiples líneas, la primera es el título. Si el modelo omite el título, todo el texto va a XTTS igualmente.
- **[Risk] Latencia adicional** → Añadir una inferencia de LLM (~2-10s) al pipeline. Aceptable: la generación del cuento ES el valor del producto, no un overhead.
- **[Risk] El modelo storyteller no responde** → El error de `IChatCompletionService` se propaga al catch del endpoint, que devuelve HTTP 500. Mismo comportamiento que cualquier otro fallo del pipeline.
