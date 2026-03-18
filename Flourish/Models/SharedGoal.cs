namespace Flourish.Models;

public class SharedGoal
{
    public Guid Id { get; set; }
    public Guid ReviewPeriodId { get; set; }
    public ReviewPeriod? ReviewPeriod { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SharedGoalMember> Members { get; set; } = [];
}

public class SharedGoalMember
{
    public Guid Id { get; set; }
    public Guid SharedGoalId { get; set; }
    public SharedGoal? SharedGoal { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
