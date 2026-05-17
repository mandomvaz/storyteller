using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.TextToAudio;

namespace Storyforge.Services;

public class VoiceStoryService
{
    private readonly Kernel _kernel;

    public VoiceStoryService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<(Guid StoryId, string Transcript)> ProcessAudioAsync(Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        var storyId = Guid.NewGuid();

        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);

        var audioService = _kernel.GetRequiredService<IAudioToTextService>();
        var audioContent = new AudioContent(ms.ToArray(), contentType);
        var result = await audioService.GetTextContentAsync(audioContent, null, _kernel, cancellationToken);

        return (storyId, result?.Text ?? string.Empty);
    }

    public async Task<(string Transcript, byte[] AudioData)> ProcessFullPipelineAsync(
        Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);

        var audioToTextService = _kernel.GetRequiredService<IAudioToTextService>();
        var audioContent = new AudioContent(ms.ToArray(), contentType);
        var result = await audioToTextService.GetTextContentAsync(audioContent, null, _kernel, cancellationToken);
        var transcript = result?.Text ?? string.Empty;

        var textToAudioService = _kernel.GetRequiredService<ITextToAudioService>();
        var speechContent = await textToAudioService.GetAudioContentAsync(transcript, cancellationToken: cancellationToken);
        var audioData = speechContent.Data?.ToArray() ?? throw new InvalidOperationException("No audio data generated.");

        return (transcript, audioData);
    }
}
