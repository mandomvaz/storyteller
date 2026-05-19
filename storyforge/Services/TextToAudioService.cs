using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using Storyforge.Hubs;
using Storyforge.Models;

namespace Storyforge.Services;

public class TextToAudioService
{
    private readonly Channel<TextUnit> _textCh;
    private readonly Channel<AudioUnit> _audioCh;
    private readonly Kernel _kernel;
    private readonly IHubContext<StoryHub> _hubContext;

    public TextToAudioService(
        Channel<TextUnit> textCh,
        Channel<AudioUnit> audioCh,
        Kernel kernel,
        IHubContext<StoryHub> hubContext)
    {
        _textCh = textCh;
        _audioCh = audioCh;
        _kernel = kernel;
        _hubContext = hubContext;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var textToAudioService = _kernel.GetRequiredService<ITextToAudioService>();

            await foreach (var textUnit in _textCh.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    var audioContent = await textToAudioService.GetAudioContentAsync(textUnit.Text, cancellationToken: cancellationToken);
                    var audioData = audioContent.Data?.ToArray() ?? throw new InvalidOperationException("No audio data generated.");
                    var audioUnit = new AudioUnit(textUnit.JobId, textUnit.ConnectionId, audioData);
                    await _audioCh.Writer.WriteAsync(audioUnit, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.Client(textUnit.ConnectionId).SendAsync("error",
                        new { jobId = textUnit.JobId, connectionId = textUnit.ConnectionId, step = "tts", message = ex.Message },
                        cancellationToken);
                    throw;
                }
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
        finally
        {
            _audioCh.Writer.Complete();
        }
    }
}
