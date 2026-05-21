namespace Storyforge.Models;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    public string Path { get; set; } = "Data/storyforge.db";
}
