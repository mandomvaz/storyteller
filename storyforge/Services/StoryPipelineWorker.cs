using System.Threading.Channels;
using Storyforge.Models;

namespace Storyforge.Services;

public class StoryPipelineWorker : BackgroundService
{
    private readonly Channel<PipelineJob> _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public StoryPipelineWorker(
        Channel<PipelineJob> channel,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<StoryPipelineRunner>();
            await runner.RunAsync(job, stoppingToken);
        }
    }
}
