using Microsoft.Extensions.Caching.Memory;
using Storyforge.Models;

namespace Storyforge.Services;

public class PersistenceService
{
    private readonly IMemoryCache _cache;
    private readonly IStoryRepository _repository;

    public PersistenceService(IMemoryCache cache, IStoryRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public Task CacheAsync(Story story)
    {
        _cache.Set(story.Id, story, TimeSpan.FromMinutes(10));
        return Task.CompletedTask;
    }

    public Story? TryGetFromCache(Guid id)
    {
        return _cache.TryGetValue(id, out Story? story) ? story : null;
    }

    public async Task<bool> PersistAsync(Guid id)
    {
        if (!_cache.TryGetValue(id, out Story? story) || story is null)
        {
            return false;
        }

        await _repository.SaveAsync(story);
        _cache.Remove(id);
        return true;
    }
}
