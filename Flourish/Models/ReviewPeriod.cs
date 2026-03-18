namespace Flourish.Models;

public class ReviewPeriod
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<ReviewEvent> ReviewEvents { get; set; } = [];
}
