namespace Storyforge.Models;

public class OllamaSettings
{
    public const string SectionName = "Ollama";

    public string Endpoint { get; set; } = "http://localhost:11434";
    public string TextModel { get; set; } = "llama3.2";
}
