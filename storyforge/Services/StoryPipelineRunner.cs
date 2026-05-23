using Microsoft.AspNetCore.SignalR;
using Storyforge.Hubs;
using Storyforge.Models;

namespace Storyforge.Services;

public class StoryPipelineRunner
{
    private readonly VoiceStoryService _voiceStoryService;
    private readonly TextToAudioService _textToAudioService;
    private readonly AudioDeliveryService _audioDeliveryService;
    private readonly IHubContext<StoryHub> _hubContext;
    private readonly BadgeService _badgeService;
    private readonly PersistenceService _persistenceService;

    public StoryPipelineRunner(
        VoiceStoryService voiceStoryService,
        TextToAudioService textToAudioService,
        AudioDeliveryService audioDeliveryService,
        IHubContext<StoryHub> hubContext,
        BadgeService badgeService,
        PersistenceService persistenceService)
    {
        _voiceStoryService = voiceStoryService;
        _textToAudioService = textToAudioService;
        _audioDeliveryService = audioDeliveryService;
        _hubContext = hubContext;
        _badgeService = badgeService;
        _persistenceService = persistenceService;
    }

    public async Task RunAsync(PipelineJob job, CancellationToken cancellationToken)
    {
        using var _ = job.AudioStream;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        var overallSw = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine($"[PIPELINE_START] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Starting Pipeline Orchestration");

        string transcript;
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[TRANSCRIPTION_START] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Sending audio to ASR (Whisper)");
            await _hubContext.Clients.Client(job.ConnectionId).SendAsync("transcribing", cts.Token);
            transcript = await _voiceStoryService.TranscribeAudioAsync(job.AudioStream, job.ContentType, cts.Token);
            sw.Stop();
            Console.WriteLine($"[TRANSCRIPTION_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Finished ASR. Duration={sw.ElapsedMilliseconds}ms. Transcript='{transcript}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TRANSCRIPTION_FAILED] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Exception={ex.Message}");
            await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error",
                new { jobId = job.JobId, connectionId = job.ConnectionId, step = "transcription", message = ex.Message },
                cts.Token);
            return;
        }

        Console.WriteLine($"[CONCURRENT_PIPELINE_START] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Launching concurrent Story LLM Generation, TTS conversion, and SignalR delivery tasks");
        await _hubContext.Clients.Client(job.ConnectionId).SendAsync("writing", cts.Token);

        var concurrentSw = System.Diagnostics.Stopwatch.StartNew();
        var storyTask = _voiceStoryService.GenerateStoryFromTextAsync(
            transcript, job.JobId.ToString(), job.ConnectionId, cts.Token);
        var ttsTask = _textToAudioService.RunAsync(cts.Token);
        var deliveryTask = _audioDeliveryService.RunAsync(cts.Token);

        var tasks = new List<Task> { storyTask, ttsTask, deliveryTask };

        try
        {
            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);
                tasks.Remove(completed);

                if (completed.IsFaulted)
                {
                    cts.Cancel();
                    await Task.WhenAll(tasks.Where(t => !t.IsCompleted));
                    var innerEx = completed.Exception!.InnerExceptions.First();
                    Console.WriteLine($"[PIPELINE_TASK_FAILED] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Task {completed.GetType().Name} failed: {innerEx.Message}");
                    throw innerEx;
                }

                if (completed == storyTask && storyTask.IsCompletedSuccessfully)
                {
                    concurrentSw.Stop();
                    Console.WriteLine($"[STORY_GENERATION_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Completed concurrent LLM Story Generation. Duration={concurrentSw.ElapsedMilliseconds}ms");
                    var story = await storyTask;

                    try
                    {
                        var badgeSw = System.Diagnostics.Stopwatch.StartNew();
                        Console.WriteLine($"[BADGE_GENERATION_START] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Generating Emojis via Semantic Kernel");
                        story.Badge = await _badgeService.GenerateBadgeAsync(story);
                        badgeSw.Stop();
                        Console.WriteLine($"[BADGE_GENERATION_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Badge='{story.Badge}'. Duration={badgeSw.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BADGE_GENERATION_FAILED] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Exception={ex.Message}");
                        await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error",
                            new { jobId = job.JobId, connectionId = job.ConnectionId, step = "badge", message = ex.Message },
                            cts.Token);
                    }

                    await _persistenceService.CacheAsync(story);
                }
                else if (completed == ttsTask && ttsTask.IsCompletedSuccessfully)
                {
                    Console.WriteLine($"[TTS_PROCESSING_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - TextToAudio worker has completed consuming all generated paragraphs");
                }
                else if (completed == deliveryTask && deliveryTask.IsCompletedSuccessfully)
                {
                    Console.WriteLine($"[DELIVERY_PROCESSING_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - AudioDelivery worker finished streaming all audio units via SignalR");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[PIPELINE_CANCELLED] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Pipeline run was cancelled");
        }
        finally
        {
            overallSw.Stop();
            Console.WriteLine($"[PIPELINE_END] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={job.JobId} - Pipeline Orchestration finished. Overall Duration={overallSw.ElapsedMilliseconds}ms");
        }
    }
}
