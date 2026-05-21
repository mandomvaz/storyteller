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
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        string transcript;
        try
        {
            await _hubContext.Clients.Client(job.ConnectionId).SendAsync("transcribing", cts.Token);
            transcript = await _voiceStoryService.TranscribeAudioAsync(job.AudioStream, job.ContentType, cts.Token);
        }
        catch (Exception ex)
        {
            await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error",
                new { jobId = job.JobId, connectionId = job.ConnectionId, step = "transcription", message = ex.Message },
                cts.Token);
            return;
        }

        await _hubContext.Clients.Client(job.ConnectionId).SendAsync("writing", cts.Token);

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
                    throw completed.Exception!.InnerExceptions.First();
                }

                if (completed == storyTask && storyTask.IsCompletedSuccessfully)
                {
                    var story = await storyTask;

                    try
                    {
                        story.Badge = await _badgeService.GenerateBadgeAsync(story);
                    }
                    catch (Exception ex)
                    {
                        await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error",
                            new { jobId = job.JobId, connectionId = job.ConnectionId, step = "badge", message = ex.Message },
                            cts.Token);
                    }

                    await _persistenceService.CacheAsync(story);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
