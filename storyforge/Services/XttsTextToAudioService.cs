using OpenAI;
using OpenAI.Audio;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;

namespace Storyforge.Services;

public class XttsTextToAudioService : ITextToAudioService
{
    private readonly AudioClient _audioClient;
    private readonly string _voice;

    public XttsTextToAudioService(OpenAIClient openAIClient, string model, string voice = "alloy")
    {
        _audioClient = openAIClient.GetAudioClient(model);
        _voice = voice;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<AudioContent> GetAudioContentAsync(
        string text,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAudioContentsAsync(text, executionSettings, kernel, cancellationToken);
        return result[0];
    }

    public async Task<IReadOnlyList<AudioContent>> GetAudioContentsAsync(
        string text,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var options = new SpeechGenerationOptions
        {
            ResponseFormat = GeneratedSpeechFormat.Wav,
        };
        GeneratedSpeechVoice voice = _voice;

        var response = await _audioClient.GenerateSpeechAsync(text, voice, options, cancellationToken);
        var audioBytes = response.Value.ToArray();

        var audioContent = new AudioContent(audioBytes, "audio/wav");
        return new List<AudioContent> { audioContent };
    }
}
