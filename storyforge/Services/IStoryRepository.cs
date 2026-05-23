using Storyforge.Models;

namespace Storyforge.Services;

public interface IStoryRepository
{
    Task InitDatabaseAsync();
    Task SaveAsync(Story story);
    Task<List<StorySummary>> GetAllAsync();
    Task<Story?> GetByIdAsync(Guid id);
    Task UpdateAsync(Story story);
    Task DeleteAsync(Guid id);
}
