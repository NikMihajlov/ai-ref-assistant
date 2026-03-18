namespace Flourish.Models;

public class ReviewEvent
{
    public Guid Id { get; set; }
    public Guid ReviewPeriodId { get; set; }
    public ReviewPeriod? ReviewPeriod { get; set; }
    public Guid RevieweeId { get; set; }
    public User? Reviewee { get; set; }
    public Guid ReviewerId { get; set; }
    public User? Reviewer { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public string? MeetLink { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
