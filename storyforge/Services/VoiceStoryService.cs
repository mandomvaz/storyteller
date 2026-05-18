using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextToAudio;

namespace Storyforge.Services;

public class VoiceStoryService
{
    private readonly Kernel _kernel;

    public VoiceStoryService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<(string Transcript, byte[] AudioData, string Title)> ProcessFullPipelineAsync(
        Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);

        // 1. Transcribe audio
        var audioToTextService = _kernel.GetRequiredService<IAudioToTextService>();
        var audioContent = new AudioContent(ms.ToArray(), contentType);
        var result = await audioToTextService.GetTextContentAsync(audioContent, null, _kernel, cancellationToken);
        var transcript = result?.Text ?? string.Empty;

        // 2. Generate story via Ollama
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(transcript);
        var storyResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);
        var storyText = storyResponse.Content ?? string.Empty;

        // 3. Extract title from first line
        var lines = storyText.Split('\n', StringSplitOptions.None);
        var title = lines.FirstOrDefault()?.Trim() ?? string.Empty;

        // 4. Generate audio from full story text
        var textToAudioService = _kernel.GetRequiredService<ITextToAudioService>();
        var speechContent = await textToAudioService.GetAudioContentAsync(storyText, cancellationToken: cancellationToken);
        var audioData = speechContent.Data?.ToArray() ?? throw new InvalidOperationException("No audio data generated.");

        return (transcript, audioData, title);
    }
}
