namespace Flourish.Models;

public enum NotificationType
{
    GoalCompleted,
    ReviewScheduled,
    ReviewReminder
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = "";
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
