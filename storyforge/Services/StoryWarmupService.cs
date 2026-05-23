using Microsoft.SemanticKernel.TextToAudio;

namespace Storyforge.Services;

public class StoryWarmupService
{
    private readonly ITextToAudioService _ttsService;
    private readonly object _lock = new();
    private Task? _warmupTask;
    private bool _warmedUpSuccessfully;

    public StoryWarmupService(ITextToAudioService ttsService)
    {
        _ttsService = ttsService;
    }

    public Task WarmupAsync()
    {
        lock (_lock)
        {
            if (_warmedUpSuccessfully)
            {
                return Task.CompletedTask;
            }

            if (_warmupTask == null || _warmupTask.IsFaulted || _warmupTask.IsCanceled)
            {
                _warmupTask = RunWarmupInternalAsync();
            }

            return _warmupTask;
        }
    }

    private async Task RunWarmupInternalAsync()
    {
        Console.WriteLine($"[WARMUP_SERVICE] [{DateTime.UtcNow:HH:mm:ss.fff}] Starting XTTS preheating warmup...");
        await _ttsService.GetAudioContentAsync("calentamiento");
        _warmedUpSuccessfully = true;
        Console.WriteLine($"[WARMUP_SERVICE] [{DateTime.UtcNow:HH:mm:ss.fff}] XTTS successfully preheated!");
    }
}
