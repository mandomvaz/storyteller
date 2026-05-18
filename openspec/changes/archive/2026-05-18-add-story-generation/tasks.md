## 1. Configuración

- [x] 1.1 Actualizar `appsettings.json`: cambiar `Ollama:TextModel` de `"llama3.2"` a `"storyteller"`
- [x] 1.2 Actualizar `Program.cs`: cambiar default de `ollamaModel` de `"llama3.2"` a `"storyteller"`

## 2. VoiceStoryService

- [x] 2.1 Eliminar método `ProcessAudioAsync` (código muerto)
- [x] 2.2 Modificar `ProcessFullPipelineAsync`: añadir paso de generación de cuento vía `IChatCompletionService` con el transcript como único mensaje de usuario
- [x] 2.3 Extraer título de la primera línea del texto generado (split por `\n`, `FirstOrDefault`)
- [x] 2.4 Enviar el texto completo (título + cuerpo) a `ITextToAudioService` y actualizar firma del método para devolver `(string Transcript, byte[] AudioData, string Title)`

## 3. Compilar y verificar

- [x] 3.1 Ejecutar `dotnet build` y corregir errores
