using System.Threading.Channels;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using Storyforge.Hubs;
using Storyforge.Models;
using Storyforge.Services;
using System.ClientModel;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection(OllamaSettings.SectionName));

builder.Services.AddScoped<VoiceStoryService>();
builder.Services.AddScoped<TextToAudioService>();
builder.Services.AddScoped<AudioDeliveryService>();
builder.Services.AddScoped<StoryPipelineRunner>();

builder.Services.AddScoped(_ => Channel.CreateUnbounded<TextUnit>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
}));
builder.Services.AddScoped(_ => Channel.CreateUnbounded<AudioUnit>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
}));

builder.Services.AddSignalR().AddMessagePackProtocol();

builder.Services.AddSingleton(Channel.CreateUnbounded<PipelineJob>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
}));

builder.Services.AddHostedService<StoryPipelineWorker>();

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

app.MapHub<StoryHub>("/storyHub");

app.MapPost("/api/stories/new", async (HttpRequest request, Channel<PipelineJob> channel,
    CancellationToken cancellationToken) =>
    {
        if (!request.HasFormContentType)
        {
            return Results.StatusCode(415);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files["audio"];
        var connectionId = form["connectionId"].FirstOrDefault();

        if (string.IsNullOrEmpty(connectionId))
        {
            return Results.BadRequest(new { error = "Connection ID is required." });
        }

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

        var stream = file.OpenReadStream();
        var job = new PipelineJob
        {
            JobId = Guid.NewGuid(),
            ConnectionId = connectionId,
            AudioStream = stream,
            ContentType = contentType
        };

        await channel.Writer.WriteAsync(job, cancellationToken);

        return Results.Ok(new { jobId = job.JobId });
    });

app.Run();
