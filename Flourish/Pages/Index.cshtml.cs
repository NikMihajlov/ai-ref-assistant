using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages;

public class IndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<Goal> MyGoals { get; set; } = [];
    public ReviewPeriod? ActivePeriod { get; set; }
    public int TotalGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int InProgressGoals { get; set; }
    public int AtRiskGoals { get; set; }
    public List<ReviewEvent> UpcomingReviews { get; set; } = [];
    public Guid CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        CurrentUserId = user.Id;

        ActivePeriod = await db.ReviewPeriods
            .Where(rp => rp.IsActive)
            .OrderByDescending(rp => rp.StartDate)
            .FirstOrDefaultAsync();

        if (ActivePeriod is not null)
        {
            MyGoals = await db.Goals
                .Where(g => g.UserId == user.Id && g.ReviewPeriodId == ActivePeriod.Id)
                .Include(g => g.Actionables)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        TotalGoals = MyGoals.Count;
        CompletedGoals = MyGoals.Count(g => g.Status == GoalStatus.Completed);
        InProgressGoals = MyGoals.Count(g => g.Status == GoalStatus.InProgress);
        AtRiskGoals = MyGoals.Count(g => g.Status == GoalStatus.AtRisk);

        UpcomingReviews = await db.ReviewEvents
            .Where(r => (r.RevieweeId == user.Id || r.ReviewerId == user.Id) && r.ScheduledAt >= DateTime.UtcNow)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Include(r => r.ReviewPeriod)
            .OrderBy(r => r.ScheduledAt)
            .Take(3)
            .ToListAsync();

        return Page();
    }
}
