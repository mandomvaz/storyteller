using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextToAudio;
using Storyforge.Models;

namespace Storyforge.Services;

public class VoiceStoryService
{
    private readonly Kernel _kernel;

    public VoiceStoryService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);

        var audioToTextService = _kernel.GetRequiredService<IAudioToTextService>();
        var audioContent = new AudioContent(ms.ToArray(), contentType);
        var result = await audioToTextService.GetTextContentAsync(audioContent, null, _kernel, cancellationToken);
        return result?.Text ?? string.Empty;
    }

    public async Task<string> GenerateStoryAsync(string transcript, CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(transcript);
        var storyResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);
        return storyResponse.Content ?? string.Empty;
    }

    public async Task<byte[]> GenerateAudioAsync(string storyText, CancellationToken cancellationToken = default)
    {
        var textToAudioService = _kernel.GetRequiredService<ITextToAudioService>();
        var speechContent = await textToAudioService.GetAudioContentAsync(storyText, cancellationToken: cancellationToken);
        return speechContent.Data?.ToArray() ?? throw new InvalidOperationException("No audio data generated.");
    }

    public async Task<(string Transcript, byte[] AudioData, string Title)> ProcessFullPipelineAsync(
        Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        var transcript = await TranscribeAudioAsync(audioStream, contentType, cancellationToken);
        var storyText = await GenerateStoryAsync(transcript, cancellationToken);

        var lines = storyText.Split('\n', StringSplitOptions.None);
        var title = lines.FirstOrDefault()?.Trim() ?? string.Empty;

        var audioData = await GenerateAudioAsync(storyText, cancellationToken);
        return (transcript, audioData, title);
    }

    public async Task<(Story Story, byte[] AudioData)> GenerateStoryFromTextAsync(
        string text, CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(text);

        var buffer = new StringBuilder();
        var story = new Story();
        var isFirstParagraph = true;

        await foreach (var chunk in chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory, cancellationToken: cancellationToken))
        {
            buffer.Append(chunk.Content ?? string.Empty);

            var textSoFar = buffer.ToString();
            var idx = textSoFar.IndexOf("\n\n", StringComparison.Ordinal);

            while (idx >= 0)
            {
                var paragraph = textSoFar[..idx].Trim();
                textSoFar = textSoFar[(idx + 2)..];
                buffer.Clear();
                buffer.Append(textSoFar);

                if (paragraph.Length > 0)
                {
                    if (isFirstParagraph)
                    {
                        story.Title = paragraph;
                        isFirstParagraph = false;
                    }
                    else
                    {
                        story.Paragraphs.Add(paragraph);
                    }
                }

                idx = textSoFar.IndexOf("\n\n", StringComparison.Ordinal);
            }
        }

        if (buffer.Length > 0)
        {
            var remaining = buffer.ToString().Trim();
            if (remaining.Length > 0)
            {
                if (isFirstParagraph)
                    story.Title = remaining;
                else
                    story.Paragraphs.Add(remaining);
            }
        }

        var ttsText = story.Paragraphs.Count > 0
            ? string.Join("\n\n", story.Title, string.Join("\n\n", story.Paragraphs))
            : story.Title;

        var textToAudioService = _kernel.GetRequiredService<ITextToAudioService>();
        var audioContent = await textToAudioService.GetAudioContentAsync(ttsText, cancellationToken: cancellationToken);
        var audioData = audioContent.Data?.ToArray() ?? throw new InvalidOperationException("No audio data generated.");

        return (story, audioData);
    }
}
