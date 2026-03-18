using Flourish.Data;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Api;

public class NotificationCountModel(AppDbContext db, CurrentUserService currentUser) : PageModel
{
    public int Count { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await currentUser.GetAsync();
        if (user is null) return Content("");

        Count = await db.Notifications.CountAsync(n => n.UserId == user.Id && !n.IsRead);
        return Page();
    }
}
