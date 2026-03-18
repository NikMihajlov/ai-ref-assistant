namespace Flourish.Models;

public enum UserRole
{
    Employee,
    Manager,
    TeamLead
}

public class User
{
    public Guid Id { get; set; }
    public string GoogleId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Reports { get; set; } = [];
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<ReviewEvent> ReviewsAsReviewee { get; set; } = [];
    public ICollection<ReviewEvent> ReviewsAsReviewer { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
