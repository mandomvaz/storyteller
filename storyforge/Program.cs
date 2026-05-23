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
Console.WriteLine("=== ANTIGRAVITY MAGIC BOOK SERVER ACTIVE ===");

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
builder.Services.AddSingleton<StoryWarmupService>();

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

app.MapPost("/api/stories/warmup", async (StoryWarmupService warmupService) =>
{
    try
    {
        Console.WriteLine($"[WARMUP_API] [{DateTime.UtcNow:HH:mm:ss.fff}] Warmup requested...");
        await warmupService.WarmupAsync();
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

app.MapGet("/api/stories", async (IStoryRepository repository) =>
{
    try
    {
        var summaries = await repository.GetAllAsync();
        return Results.Ok(summaries);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GET_STORIES_FAILED] {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/stories/{id:guid}", async (Guid id, IStoryRepository repository) =>
{
    try
    {
        var story = await repository.GetByIdAsync(id);
        return story is not null ? Results.Ok(story) : Results.NotFound(new { error = "Story not found" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GET_STORY_FAILED] StoryId={id} - {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/stories/{id:guid}/replay", async (
    Guid id,
    string connectionId,
    IStoryRepository repository,
    Channel<TextUnit> textCh,
    Channel<AudioUnit> audioCh,
    TextToAudioService ttsService,
    AudioDeliveryService deliveryService) =>
{
    var story = await repository.GetByIdAsync(id);
    if (story is null)
    {
        return Results.NotFound(new { error = "Story not found" });
    }

    if (string.IsNullOrEmpty(connectionId))
    {
        return Results.BadRequest(new { error = "Connection ID is required." });
    }

    var jobIdStr = id.ToString();
    Console.WriteLine($"[REPLAY_START] StoryId={id} - Replaying voice streaming for connection={connectionId}");

    var cts = new CancellationTokenSource();
    var ttsTask = ttsService.RunAsync(cts.Token);
    var deliveryTask = deliveryService.RunAsync(cts.Token);

    try
    {
        // Enqueue the title of the story first
        await textCh.Writer.WriteAsync(new TextUnit(jobIdStr, connectionId, story.Title), cts.Token);

        // Enqueue subsequent paragraphs
        foreach (var paragraph in story.Paragraphs)
        {
            await textCh.Writer.WriteAsync(new TextUnit(jobIdStr, connectionId, paragraph), cts.Token);
        }

        // Close the text channel writer to let the TTS service finish
        textCh.Writer.Complete();

        // Await the TTS and SignalR delivery workers
        await Task.WhenAll(ttsTask, deliveryTask);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[REPLAY_FAILED] StoryId={id} ConnectionId={connectionId} - Exception={ex.Message}");
        cts.Cancel();
        return Results.Problem(ex.Message);
    }

    return Results.Ok(new { replayed = true });
});

app.MapPut("/api/stories/{id:guid}", async (Guid id, Story updatedStory, IStoryRepository repository) =>
{
    try
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Results.NotFound(new { error = "Cuento no encontrado" });
        }

        existing.Title = updatedStory.Title;
        existing.Badge = updatedStory.Badge;
        existing.Paragraphs = updatedStory.Paragraphs;

        await repository.UpdateAsync(existing);
        return Results.Ok(new { updated = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[UPDATE_STORY_FAILED] StoryId={id} - {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("/api/stories/{id:guid}", async (Guid id, IStoryRepository repository) =>
{
    try
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Results.NotFound(new { error = "Cuento no encontrado" });
        }

        await repository.DeleteAsync(id);
        return Results.Ok(new { deleted = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DELETE_STORY_FAILED] StoryId={id} - {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/stories/tts", async (
    TtsRequest request,
    ITextToAudioService ttsService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.BadRequest(new { error = "El texto es requerido" });
        }

        // Utiliza estrictamente la abstracción oficial de Semantic Kernel
        var audioContent = await ttsService.GetAudioContentAsync(request.Text, cancellationToken: cancellationToken);
        if (audioContent?.Data is null)
        {
            return Results.Problem("Error en la síntesis de voz a través de Semantic Kernel");
        }

        return Results.File(audioContent.Data.Value.ToArray(), "audio/wav");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SINGLE_TTS_FAILED] {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.Run();

public record TtsRequest(string Text);
