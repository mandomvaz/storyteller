using System.Text;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Storyforge.Hubs;
using Storyforge.Models;

namespace Storyforge.Services;

public class VoiceStoryService
{
    private readonly Kernel _kernel;
    private readonly Channel<TextUnit> _textCh;
    private readonly IHubContext<StoryHub> _hubContext;

    public VoiceStoryService(
        Kernel kernel,
        Channel<TextUnit> textCh,
        IHubContext<StoryHub> hubContext)
    {
        _kernel = kernel;
        _textCh = textCh;
        _hubContext = hubContext;
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string contentType, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);

        var audioToTextService = _kernel.GetRequiredService<IAudioToTextService>();
        var audioContent = new AudioContent(ms.ToArray(), contentType);
        
        var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIAudioToTextExecutionSettings("audio.webm")
        {
            Language = "es"
        };

        var result = await audioToTextService.GetTextContentAsync(audioContent, executionSettings, _kernel, cancellationToken);
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

    public async Task<Story> GenerateStoryFromTextAsync(
        string text, string jobId, string connectionId, CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(text);

        var buffer = new StringBuilder();
        var story = new Story { Id = Guid.Parse(jobId) };
        var isFirstParagraph = true;

        try
        {
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
                        var pSw = System.Diagnostics.Stopwatch.StartNew();
                        Console.WriteLine($"[PARAGRAPH_GENERATED] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={jobId} - Writing paragraph to channel. Length={paragraph.Length} chars");
                        if (isFirstParagraph)
                        {
                            story.Title = paragraph;
                            isFirstParagraph = false;
                        }
                        else
                        {
                            story.Paragraphs.Add(paragraph);
                        }

                        await _textCh.Writer.WriteAsync(
                            new TextUnit(jobId, connectionId, paragraph), cancellationToken);
                        pSw.Stop();
                    }

                    idx = textSoFar.IndexOf("\n\n", StringComparison.Ordinal);
                }
            }

            if (buffer.Length > 0)
            {
                var remaining = buffer.ToString().Trim();
                if (remaining.Length > 0)
                {
                    Console.WriteLine($"[FINAL_PARAGRAPH_FLUSH] [{DateTime.UtcNow:HH:mm:ss.fff}] JobId={jobId} - Writing final remaining paragraph to channel. Length={remaining.Length} chars");
                    if (isFirstParagraph)
                        story.Title = remaining;
                    else
                        story.Paragraphs.Add(remaining);

                    await _textCh.Writer.WriteAsync(
                        new TextUnit(jobId, connectionId, remaining), cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("error",
                new { jobId, connectionId, step = "story", message = ex.Message },
                cancellationToken);
            throw;
        }
        finally
        {
            _textCh.Writer.Complete();
        }

        return story;
    }
}
