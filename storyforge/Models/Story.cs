namespace Storyforge.Models;

public class Story
{
    public string Title { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public List<string> Paragraphs { get; set; } = new();
}
