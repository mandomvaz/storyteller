using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Storyforge.Hubs;
using Storyforge.Models;

namespace Storyforge.Services;

public class AudioDeliveryService
{
    private readonly Channel<AudioUnit> _audioCh;
    private readonly IHubContext<StoryHub> _hubContext;

    public AudioDeliveryService(
        Channel<AudioUnit> audioCh,
        IHubContext<StoryHub> hubContext)
    {
        _audioCh = audioCh;
        _hubContext = hubContext;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var connectionId = "";

        try
        {
            await foreach (var audioUnit in _audioCh.Reader.ReadAllAsync(cancellationToken))
            {
                connectionId = audioUnit.ConnectionId;

                try
                {
                    await _hubContext.Clients.Client(audioUnit.ConnectionId).SendAsync("audioChunk", audioUnit.AudioData, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.Client(audioUnit.ConnectionId).SendAsync("error",
                        new { jobId = audioUnit.JobId, connectionId = audioUnit.ConnectionId, step = "delivery", message = ex.Message },
                        cancellationToken);
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("storyComplete", cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            throw;
        }
    }
}
