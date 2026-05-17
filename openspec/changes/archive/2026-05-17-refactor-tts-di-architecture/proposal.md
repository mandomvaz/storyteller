## Why

La implementación actual del TTS registra `XttsTextToAudioService` en `builder.Services` (DI estándar de ASP.NET), mientras que el STT se registra dentro del flujo de `AddKernel()` de Semantic Kernel. Esto rompe la consistencia arquitectónica: `VoiceStoryService` resuelve servicios AI vía `kernel.GetRequiredService<>()` pero no podría resolver TTS de la misma forma. El endpoint se ve forzado a inyectar `ITextToAudioService` directamente, dividiendo la orquestación del pipeline en dos lugares.

## What Changes

- **Modificado**: `Program.cs` — mover registro de `ITextToAudioService` al flujo del Kernel builder (`AddKernel().AddSingleton<ITextToAudioService>(...)`), consistente con `AddOpenAIAudioToText`
- **Modificado**: `VoiceStoryService` — añadir orquestación de TTS (pipeline completo: STT → TTS), resolviendo ambos servicios vía `_kernel.GetRequiredService<>()`
- **Modificado**: Endpoint `POST /api/stories/new` — eliminar inyección directa de `ITextToAudioService`, delegar pipeline completo a `VoiceStoryService`
- Sin cambios en `XttsTextToAudioService.cs`, `appsettings.json`, ni specs existentes (los specs ya especificaban correctamente el registro en el Kernel)

## Capabilities

### New Capabilities
- *(ninguna — es un refactor de la implementación existente)*

### Modified Capabilities
- *(ninguna — los specs existentes ya definen el comportamiento correcto; solo se alinea la implementación)*

## Impact

- `storyforge/Program.cs`: Reordenar registro de TTS para que pase por el Kernel builder
- `storyforge/Services/VoiceStoryService.cs`: Añadir método para pipeline completo con TTS; eliminar dependencia directa de `ITextToAudioService` en el endpoint
- Sin nuevos paquetes NuGet
- Sin cambios en frontend
- Sin cambios en configuración
