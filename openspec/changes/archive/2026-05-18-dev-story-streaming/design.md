## Context

Actualmente el pipeline completo es `Audio → Whisper → Ollama → XTTS` y todo es síncrono (espera a que cada paso termine). Para desarrollo, necesitamos un atajo que salte Whisper y además sirva de banco de pruebas para el streaming de Ollama.

Nace el modelo `Story` que captura el resultado estructurado de Ollama: título, badges (emojis, reservado para futuro), y párrafos. El streaming de Ollama rellena este objeto progresivamente. Una vez completo, se envía el texto completo a XTTS.

## Goals / Non-Goals

**Goals:**
- Crear endpoint `/api/stories/test` con texto fijo, sin Whisper
- Modelo `Story` con Title, Badge, Paragraphs
- Stream de Ollama token a token vía `IChatCompletionService.GetStreamingChatMessageContentsAsync`
- Rellenar `Story` progresivamente: detectar `\n\n` para separar párrafos, primera línea = título
- Al completar el stream, concatenar título + párrafos y enviar a XTTS
- Botón en frontend que dispare el test
- Debe ser fácil de eliminar cuando se integre la feature real

**Non-Goals:**
- No se modifica el endpoint `/api/stories/new`
- No se añaden paquetes NuGet nuevos
- No se toca `XttsTextToAudioService`
- No se hace aún el envío paralelo de párrafos a XTTS (se envía todo junto al final)

## Decisions

### 1. Story como modelo simple

```csharp
public class Story
{
    public string Title { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;  // emojis, para futuro
    public List<string> Paragraphs { get; set; } = new();
}
```

POCO sin lógica. Se rellena desde el stream de Ollama.

### 2. Stream de Ollama → Story

```csharp
var chatService = _kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
chatHistory.AddUserMessage(text);

var story = new Story();
var buffer = new StringBuilder();

await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory))
{
    var token = chunk.Content ?? string.Empty;
    
    // Detectar doble salto de línea = fin de párrafo
    if (token.Contains("\n\n"))
    {
        // Partir por \n\n, la primera parte va al párrafo actual
        // Si es el primer párrafo, es el título
        // ... lógica de acumulación
    }
    
    buffer.Append(token);
}

// Flush final
```

Alternativa descartada: `OllamaSharp.Chat.SendAsync` — más simple pero nos saltamos la abstracción de SK.

### 3. Detección de título

La primera línea del texto generado por `storyteller` es el título. Con el stream:
- El primer párrafo acumulado (todo hasta el primer `\n\n`) es el título
- El texto del título se guarda en `Story.Title` y también se incluye en el texto final que va a XTTS

### 4. Sin WAV concatenation

A diferencia de lo que se exploró, el pipeline final es:
```
Ollama stream → rellenar Story (título + párrafos) → XTTS(texto_completo) → audio
```

No se envía cada párrafo por separado a XTTS aún, así que no hay que concatenar WAVs. Esto se puede explorar en un cambio futuro.

### 5. Endpoint de prueba

```
POST /api/stories/test
Content-Type: application/json

{ "text": "Había una vez un perro llamado Max" }
```

Devuelve:
```json
{
    "title": "Max y la aventura...",
    "badge": "",
    "paragraphs": ["...", "..."],
    "audioUrl": "/audio/..."
}
```

Opcionalmente, en vez de devolver JSON con URL, se puede devolver el WAV directamente como hace el endpoint actual. Depende de si queremos mostrar el título en el frontend.

### 6. Temporal

Todo el código del endpoint y botón se marca en el frontend y Program.cs de forma que sea fácil de identificar y eliminar. No se usan `#if DEBUG` porque el usuario prefiere control manual.

## Risks / Trade-offs

- **[Risk] El stream de Ollama devuelve tokens a media palabra** → El detector de `\n\n` puede partir un token justo en el salto de línea. Hay que manejar el caso donde `\n\n` aparece en medio de un token (`.Contains("\n\n")`).
- **[Risk] Latencia de XTTS al final** → Toda la latencia de TTS está al final, no se gana paralelismo. Aceptable: esta es la Fase 1. La Fase 2 explorará enviar párrafos a XTTS conforme llegan.
- **[Risk] Primer párrafo muy largo** → El título se define como todo hasta el primer `\n\n`. Si el modelo no pone `\n\n` hasta muy tarde, el título será muy largo. Aceptable para dev, se ajustará el prompt del modelo si es necesario.
