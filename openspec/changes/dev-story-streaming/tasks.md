## 1. Modelo Story

- [ ] 1.1 Crear `Models/Story.cs` con Title, Badge (string), Paragraphs (List\<string\>)

## 2. Streaming en VoiceStoryService

- [ ] 2.1 Añadir método `GenerateStoryFromTextAsync(string text, CancellationToken)` que recibe texto, usa `GetStreamingChatMessageContentsAsync` para stream de Ollama, acumula tokens, detecta `\n\n` para párrafos, rellena `Story`
- [ ] 2.2 Al completar el stream, concatenar título + párrafos y enviar a `ITextToAudioService.GetAudioContentAsync`
- [ ] 2.3 Devolver `(Story story, byte[] AudioData)` desde el método

## 3. Endpoint de prueba

- [ ] 3.1 Añadir `POST /api/stories/test` en Program.cs que acepte texto fijo, llame a `GenerateStoryFromTextAsync`, devuelva el audio WAV

## 4. Frontend

- [ ] 4.1 Añadir botón en `wwwroot/index.html` que POSTee a `/api/stories/test` con texto fijo y reproduzca el audio resultante

## 5. Compilar y verificar

- [ ] 5.1 Ejecutar `dotnet build` y corregir errores
