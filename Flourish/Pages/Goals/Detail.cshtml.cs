using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Goals;

public class GoalDetailModel(CurrentUserService currentUserService, AppDbContext db, NotificationService notifications) : FlourishPageModel(currentUserService)
{
    public Goal? Goal { get; set; }
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        Goal = await db.Goals
            .Include(g => g.Actionables)
            .Include(g => g.ReviewPeriod)
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (Goal is null) return NotFound();
        if (!CurrentUserService.CanViewGoal(user, Goal)) return Forbid();

        CanEdit = Goal.UserId == user.Id;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, GoalStatus status)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        var goal = await db.Goals.Include(g => g.User).FirstOrDefaultAsync(g => g.Id == id);
        if (goal is null || goal.UserId != user.Id) return Forbid();

        var wasCompleted = goal.Status == GoalStatus.Completed;
        goal.Status = status;
        goal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Notify manager and team lead when goal is marked completed
        if (!wasCompleted && status == GoalStatus.Completed)
        {
            var teamLead = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.TeamLead);
            if (teamLead is not null)
                await notifications.NotifyGoalCompletedAsync(goal, teamLead);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        var goal = await db.Goals.FindAsync(id);
        if (goal is null || goal.UserId != user.Id) return Forbid();

        db.Goals.Remove(goal);
        await db.SaveChangesAsync();
        return RedirectToPage("/Goals/Index");
    }
}
