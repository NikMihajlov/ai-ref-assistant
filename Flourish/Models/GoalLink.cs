namespace Flourish.Models;

public class GoalLink
{
    public Guid Id { get; set; }
    public Guid GoalId1 { get; set; }
    public Goal? Goal1 { get; set; }
    public Guid GoalId2 { get; set; }
    public Goal? Goal2 { get; set; }
    public string? Note { get; set; }
    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
