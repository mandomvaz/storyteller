## 1. Registrar TTS en el Kernel builder

- [x] 1.1 En `Program.cs`, capturar el `IKernelBuilder` devuelto por `AddKernel()` en una variable
- [x] 1.2 Mover `AddSingleton<ITextToAudioService>` al `kernelBuilder.Services` en vez de `builder.Services`
- [x] 1.3 Verificar que `kernelBuilder.Services.AddSingleton<ITextToAudioService>(...)` compile sin errores

## 2. Ampliar VoiceStoryService con TTS

- [x] 2.1 Añadir método `ProcessFullPipelineAsync(Stream audioStream, string contentType, CancellationToken)` que devuelva `(string Transcript, byte[] AudioData)`
- [x] 2.2 Dentro del método: resolver `IAudioToTextService` y `ITextToAudioService` vía `_kernel.GetRequiredService<>()`
- [x] 2.3 Ejecutar STT → TTS en secuencia y devolver el resultado

## 3. Simplificar el endpoint

- [x] 3.1 Eliminar el parámetro `ITextToAudioService ttsService` del endpoint
- [x] 3.2 Llamar a `voiceStoryService.ProcessFullPipelineAsync()` en vez de hacer STT + TTS por separado
- [x] 3.3 Devolver `Results.File(audioData, "audio/wav")` con los bytes del pipeline completo

## 4. Compilar y verificar

- [x] 4.1 Ejecutar `dotnet build` y corregir errores de compilación
- [x] 4.2 Verificar que el endpoint responde correctamente con el pipeline unificado
