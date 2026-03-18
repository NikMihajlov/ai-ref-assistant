namespace Flourish.Models;

public class Actionable
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public Goal? Goal { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
