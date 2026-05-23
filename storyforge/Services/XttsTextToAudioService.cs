using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;

namespace Storyforge.Services;

public class XttsTextToAudioService : ITextToAudioService
{
    private readonly HttpClient _httpClient;
    private readonly string _speaker;
    private readonly string _language;

    public XttsTextToAudioService(HttpClient httpClient, string speaker, string language = "es")
    {
        _httpClient = httpClient;
        _speaker = speaker;
        _language = language;
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
        var requestBody = new
        {
            text,
            speaker_wav = _speaker,
            language = _language
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine($"[XTTS_API_START] [{DateTime.UtcNow:HH:mm:ss.fff}] TTS request starting. TextLength={text.Length} chars - '{text[..Math.Min(25, text.Length)]}...'");

        var response = await _httpClient.PostAsJsonAsync("tts_to_audio", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<TtsResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from TTS server");

        // El servidor XTTS en el host devuelve una URL absoluta usando "localhost" (ej. http://localhost:8020/temp/xxx.wav).
        // Dado que estamos dentro de un contenedor Docker, resolver "localhost" dentro del contenedor fallará.
        // Extraemos solo la ruta relativa para realizar la descarga utilizando la IP real configurada en el HttpClient.
        string downloadPath = responseBody.Url;
        if (Uri.TryCreate(responseBody.Url, UriKind.Absolute, out var absoluteUri))
        {
            downloadPath = absoluteUri.PathAndQuery.TrimStart('/');
        }

        var audioBytes = await _httpClient.GetByteArrayAsync(downloadPath, cancellationToken);
        sw.Stop();
        Console.WriteLine($"[XTTS_API_COMPLETE] [{DateTime.UtcNow:HH:mm:ss.fff}] TTS response fetched. AudioBytes={audioBytes.Length}. Duration={sw.ElapsedMilliseconds}ms");

        var audioContent = new AudioContent(audioBytes, "audio/wav");
        return new List<AudioContent> { audioContent };
    }

    private sealed class TtsResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
