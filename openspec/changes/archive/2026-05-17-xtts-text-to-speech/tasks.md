## 1. Configuración

- [x] 1.1 Añadir sección `Xtts` a `appsettings.json` con `Endpoint`, `Model` y `Voice`
- [x] 1.2 Leer configuración XTTS en `Program.cs` (mismo patrón que whisper: `builder.Configuration["Xtts:Endpoint"]`, etc.)
- [x] 1.3 Crear `OpenAIClient` apuntando a `{Xtts:Endpoint}/v1` (mismo truco del `/v1` que whisper)

## 2. Servicio TTS

- [x] 2.1 Crear `Services/XttsTextToAudioService.cs` implementando `ITextToAudioService`
- [x] 2.2 En el constructor: recibir `OpenAIClient`, crear `AudioClient` interno
- [x] 2.3 Implementar `GetAudioContentsAsync()` usando `AudioClient.GenerateSpeechAsync()`
- [x] 2.4 Registrar `XttsTextToAudioService` en el Kernel como `ITextToAudioService`

## 3. Endpoint modificado

- [x] 3.1 Modificar endpoint `POST /api/stories/new`: transcribir con STT, pasar texto a TTS, devolver `Results.File(byte[], "audio/mpeg")`
- [x] 3.2 Verificar que el pipeline completo responde sin errores
