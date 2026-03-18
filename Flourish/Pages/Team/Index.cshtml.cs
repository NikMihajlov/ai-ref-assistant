using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Team;

public class TeamIndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<User> TeamMembers { get; set; } = [];
    public Dictionary<Guid, List<Goal>> GoalsByUser { get; set; } = [];
    public List<ReviewPeriod> Periods { get; set; } = [];
    public Guid? SelectedPeriodId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? periodId)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role is not (UserRole.Manager or UserRole.TeamLead)) return Forbid();

        Periods = await db.ReviewPeriods.OrderByDescending(rp => rp.StartDate).ToListAsync();
        SelectedPeriodId = periodId ?? Periods.FirstOrDefault(p => p.IsActive)?.Id ?? Periods.FirstOrDefault()?.Id;

        TeamMembers = user.Role == UserRole.TeamLead
            ? await db.Users.Where(u => u.Id != user.Id).OrderBy(u => u.Name).ToListAsync()
            : await db.Users.Where(u => u.ManagerId == user.Id).OrderBy(u => u.Name).ToListAsync();

        if (SelectedPeriodId.HasValue)
        {
            var memberIds = TeamMembers.Select(m => m.Id).ToList();
            var goals = await db.Goals
                .Where(g => memberIds.Contains(g.UserId) && g.ReviewPeriodId == SelectedPeriodId)
                .Include(g => g.Actionables)
                .Include(g => g.User)
                .ToListAsync();

            GoalsByUser = goals
                .GroupBy(g => g.UserId)
                .ToDictionary(gr => gr.Key, gr => gr.ToList());
        }

        return Page();
    }
}
