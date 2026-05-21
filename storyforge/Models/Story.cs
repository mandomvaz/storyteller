using System.Text.Json;

namespace Storyforge.Models;

public class Story
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public List<string> Paragraphs { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SerializeParagraphs() =>
        JsonSerializer.Serialize(Paragraphs);

    public static List<string> DeserializeParagraphs(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
