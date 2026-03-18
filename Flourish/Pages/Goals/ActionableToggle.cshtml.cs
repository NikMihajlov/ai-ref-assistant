using Flourish.Data;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Goals;

public class ActionableToggleModel(AppDbContext db, CurrentUserService currentUser) : PageModel
{
    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var user = await currentUser.GetRequiredAsync();
        var actionable = await db.Actionables.Include(a => a.Goal).FirstOrDefaultAsync(a => a.Id == id);
        if (actionable is null || actionable.Goal?.UserId != user.Id) return Forbid();

        actionable.IsCompleted = !actionable.IsCompleted;
        await db.SaveChangesAsync();

        return Partial("_ActionableRow", actionable);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var user = await currentUser.GetRequiredAsync();
        var actionable = await db.Actionables.Include(a => a.Goal).FirstOrDefaultAsync(a => a.Id == id);
        if (actionable is null || actionable.Goal?.UserId != user.Id) return Forbid();

        db.Actionables.Remove(actionable);
        await db.SaveChangesAsync();

        return Content("");
    }
}
