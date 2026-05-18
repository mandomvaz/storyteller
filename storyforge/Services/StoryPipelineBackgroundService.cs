using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Storyforge.Hubs;
using Storyforge.Models;

namespace Storyforge.Services;

public class StoryPipelineBackgroundService : BackgroundService
{
    private readonly Channel<PipelineJob> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<StoryHub> _hubContext;

    public StoryPipelineBackgroundService(
        Channel<PipelineJob> channel,
        IServiceScopeFactory scopeFactory,
        IHubContext<StoryHub> hubContext)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var voiceStoryService = scope.ServiceProvider.GetRequiredService<VoiceStoryService>();

            string transcript;
            try
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("transcribing", stoppingToken);
                transcript = await voiceStoryService.TranscribeAudioAsync(job.AudioStream, job.ContentType, stoppingToken);
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error", new { step = "transcription", message = ex.Message }, stoppingToken);
                continue;
            }

            string storyText;
            try
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("writing", stoppingToken);
                storyText = await voiceStoryService.GenerateStoryAsync(transcript, stoppingToken);
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error", new { step = "story", message = ex.Message }, stoppingToken);
                continue;
            }

            byte[] audioData;
            try
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("generatingAudio", stoppingToken);
                audioData = await voiceStoryService.GenerateAudioAsync(storyText, stoppingToken);
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(job.ConnectionId).SendAsync("error", new { step = "audio", message = ex.Message }, stoppingToken);
                continue;
            }

            await _hubContext.Clients.Client(job.ConnectionId).SendAsync("audioReady", audioData, stoppingToken);
        }
    }
}
