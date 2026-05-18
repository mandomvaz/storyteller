namespace Storyforge.Models;

public class PipelineJob
{
    public Guid JobId { get; set; }
    public required string ConnectionId { get; set; }
    public required Stream AudioStream { get; set; }
    public required string ContentType { get; set; }
}
