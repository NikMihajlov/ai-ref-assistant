using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Goals;

public class ActionablesApiModel(AppDbContext db, CurrentUserService currentUser) : PageModel
{
    // POST /Goals/{goalId}/Actionables — add actionable, return partial HTML
    public async Task<IActionResult> OnPostAsync(Guid goalId, string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest();

        var user = await currentUser.GetRequiredAsync();
        var goal = await db.Goals.FindAsync(goalId);
        if (goal is null || goal.UserId != user.Id) return Forbid();

        var actionable = new Actionable { GoalId = goalId, Title = title.Trim() };
        db.Actionables.Add(actionable);
        await db.SaveChangesAsync();

        return Partial("_ActionableRow", actionable);
    }
}
