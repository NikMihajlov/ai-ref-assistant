using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Goals;

public class GoalsIndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<Goal> Goals { get; set; } = [];
    public List<ReviewPeriod> Periods { get; set; } = [];
    public Guid? SelectedPeriodId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? periodId)
    {
        var user = await CurrentUserService.GetRequiredAsync();

        Periods = await db.ReviewPeriods.OrderByDescending(rp => rp.StartDate).ToListAsync();

        SelectedPeriodId = periodId ?? Periods.FirstOrDefault(p => p.IsActive)?.Id ?? Periods.FirstOrDefault()?.Id;

        if (SelectedPeriodId.HasValue)
        {
            Goals = await db.Goals
                .Where(g => g.UserId == user.Id && g.ReviewPeriodId == SelectedPeriodId)
                .Include(g => g.Actionables)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        return Page();
    }
}
