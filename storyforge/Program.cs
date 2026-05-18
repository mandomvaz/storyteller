using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using Storyforge.Models;
using Storyforge.Services;
using System.ClientModel;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection(OllamaSettings.SectionName));

builder.Services.AddScoped<VoiceStoryService>();

var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
var ollamaModel = builder.Configuration["Ollama:TextModel"] ?? "storyteller";

var whisperEndpoint = builder.Configuration["Whisper:Endpoint"] ?? "http://localhost:8000";
var whisperModel = builder.Configuration["Whisper:Model"] ?? "medium";

var whisperBaseUrl = whisperEndpoint.TrimEnd('/') + "/v1";
var whisperOptions = new OpenAIClientOptions { Endpoint = new Uri(whisperBaseUrl) };
var whisperClient = new OpenAIClient(new ApiKeyCredential("sk-faster-whisper-local"), whisperOptions);

var xttsEndpoint = builder.Configuration["Xtts:Endpoint"] ?? "http://localhost:8020";
var xttsSpeaker = builder.Configuration["Xtts:Speaker"] ?? "alloy";
var xttsLanguage = builder.Configuration["Xtts:Language"] ?? "es";
var xttsClient = new HttpClient { BaseAddress = new Uri(xttsEndpoint.TrimEnd('/') + "/") };

var kernelBuilder = builder.Services.AddKernel()
    .AddOllamaChatCompletion(ollamaModel, new Uri(ollamaEndpoint))
    .AddOpenAIAudioToText(whisperModel, whisperClient);

kernelBuilder.Services.AddSingleton<ITextToAudioService>(sp =>
    new XttsTextToAudioService(xttsClient, xttsSpeaker, xttsLanguage));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/stories/new", async (HttpRequest request, VoiceStoryService voiceStoryService,
    CancellationToken cancellationToken) =>
    {
        if (!request.HasFormContentType)
        {
            return Results.StatusCode(415);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files["audio"];

        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "Audio file is required." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return Results.StatusCode(413);
        }

        var contentType = file.ContentType;
        if (contentType is null || !contentType.StartsWith("audio/"))
        {
            return Results.StatusCode(415);
        }

        await using var stream = file.OpenReadStream();

        try
        {
            var (transcript, audioData, _) = await voiceStoryService.ProcessFullPipelineAsync(stream, contentType, cancellationToken);
            return Results.File(audioData, "audio/wav");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Audio pipeline failed"
            );
        }
    });

app.Run();
