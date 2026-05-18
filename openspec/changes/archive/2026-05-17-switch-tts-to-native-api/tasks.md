## 1. Configuración

- [x] 1.1 Actualizar `appsettings.json`: renombrar `Xtts:Voice` → `Xtts:Speaker`, añadir `Xtts:Language: "es"`, eliminar `Xtts:Model`
- [x] 1.2 Actualizar `Program.cs`: eliminar `OpenAIClient` + `ApiKeyCredential` para XTTS; añadir `HttpClient` con base en `Xtts:Endpoint`; pasar `speaker` y `language` al servicio
- [x] 1.3 Eliminar `using OpenAI` y `using System.ClientModel` de `Program.cs` si whisper no los necesita (no requerido - whisper sigue usando ambas)

## 2. Reescribir XttsTextToAudioService

- [x] 2.1 Cambiar constructor: recibir `HttpClient`, `speaker` (string), `language` (string); eliminar `OpenAIClient`
- [x] 2.2 Implementar `GetAudioContentsAsync`: `POST /tts_to_audio/` con body `{ text, speaker_wav, language }`
- [x] 2.3 Parsear JSON de respuesta para extraer `url`
- [x] 2.4 Hacer `GET` a la URL devuelta para descargar el WAV binario
- [x] 2.5 Devolver `AudioContent(byte[], "audio/wav")` con los datos descargados

## 3. Compilar y verificar

- [x] 3.1 Ejecutar `dotnet build` y corregir errores
