namespace Flourish.Models;

public enum GoalStatus
{
    NotStarted,
    InProgress,
    AtRisk,
    Completed
}

public class Goal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ReviewPeriodId { get; set; }
    public ReviewPeriod? ReviewPeriod { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public bool IsPrivate { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Actionable> Actionables { get; set; } = [];
}
