namespace Storyforge.Services;

public class VoiceStoryService
{
    public Task<Guid> ProcessAudioAsync(Stream audioStream, string contentType)
    {
        var storyId = Guid.NewGuid();
        return Task.FromResult(storyId);
    }
}
