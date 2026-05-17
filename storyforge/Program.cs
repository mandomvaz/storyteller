using Storyforge.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<VoiceStoryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/stories/new", async (HttpRequest request, VoiceStoryService voiceStoryService) =>
    {
        if (!request.HasFormContentType)
        {
            return Results.StatusCode(415);
        }

        var form = await request.ReadFormAsync();
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
        var storyId = await voiceStoryService.ProcessAudioAsync(stream, contentType);

        return Results.Ok(new { storyId });
    });

app.Run();