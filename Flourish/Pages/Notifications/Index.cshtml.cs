using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Notifications;

public class NotificationsIndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<Notification> Notifications { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();

        Notifications = await db.Notifications
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        // Mark all as read on page view
        await db.Notifications
            .Where(n => n.UserId == user.Id && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        return Page();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        await db.Notifications
            .Where(n => n.UserId == user.Id && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return RedirectToPage();
    }
}
