using System.Threading.Channels;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using Storyforge.Hubs;
using Storyforge.Models;
using Storyforge.Services;
using System.ClientModel;
using OpenAI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection(OllamaSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<BadgePromptSettings>(builder.Configuration.GetSection(BadgePromptSettings.SectionName));

builder.Services.AddSingleton<IStoryRepository>(sp =>
    new SqliteStoryRepository(builder.Configuration
        .GetSection(DatabaseSettings.SectionName)
        .Get<DatabaseSettings>() ?? new DatabaseSettings()));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<PersistenceService>();
builder.Services.AddSingleton<BadgeService>();

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

var badgePromptPath = builder.Configuration["BadgePrompt:Path"] ?? "Prompts/badge.txt";
var badgePromptText = "Genera exactamente 5 emojis que representen el tono de esta historia. Devuelve solo los 5 emojis, nada más.";
if (File.Exists(badgePromptPath))
{
    badgePromptText = File.ReadAllText(badgePromptPath);
}
else
{
    Console.WriteLine($"WARNING: Badge prompt file not found at {badgePromptPath}, using default prompt");
}
var badgeFunc = KernelFunctionFactory.CreateFromPrompt(badgePromptText,
    new OpenAIPromptExecutionSettings { MaxTokens = 15 });
builder.Services.AddKeyedSingleton<KernelFunction>("badge", badgeFunc);

var app = builder.Build();

// Initialize database on startup
var repository = app.Services.GetRequiredService<IStoryRepository>();
await repository.InitDatabaseAsync();

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

        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var job = new PipelineJob
        {
            JobId = Guid.NewGuid(),
            ConnectionId = connectionId,
            AudioStream = memoryStream,
            ContentType = contentType
        };

        await channel.Writer.WriteAsync(job, cancellationToken);

        return Results.Ok(new { jobId = job.JobId });
    });

app.MapPost("/api/stories/warmup", async (ITextToAudioService ttsService) =>
{
    try
    {
        Console.WriteLine($"[WARMUP_API] [{DateTime.UtcNow:HH:mm:ss.fff}] Triggering blocking under-demand XTTS preheating...");
        await ttsService.GetAudioContentAsync("calentamiento");
        Console.WriteLine($"[WARMUP_API] [{DateTime.UtcNow:HH:mm:ss.fff}] XTTS successfully preheated!");
        return Results.Ok(new { warmed = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARMUP_API_FAILED] [{DateTime.UtcNow:HH:mm:ss.fff}] Warmup failed: {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/stories/{id:guid}/save", async (Guid id, PersistenceService persistenceService) =>
{
    var saved = await persistenceService.PersistAsync(id);
    return saved ? Results.Ok(new { saved = true }) : Results.NotFound(new { error = "Story not found or expired" });
});

app.Run();
