using Flourish.Data;
using Flourish.Models;

namespace Flourish.Services;

public class NotificationService(AppDbContext db)
{
    public async Task NotifyAsync(Guid userId, NotificationType type, string message, string? linkUrl = null)
    {
        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            LinkUrl = linkUrl
        });
        await db.SaveChangesAsync();
    }

    public async Task NotifyGoalCompletedAsync(Goal goal, User teamLead)
    {
        var link = $"/Goals/{goal.Id}";
        await NotifyAsync(teamLead.Id, NotificationType.GoalCompleted,
            $"{goal.User?.Name} marked \"{goal.Title}\" as completed.", link);

        if (goal.User?.ManagerId.HasValue == true)
        {
            await NotifyAsync(goal.User.ManagerId.Value, NotificationType.GoalCompleted,
                $"{goal.User.Name} marked \"{goal.Title}\" as completed.", link);
        }
    }
}
