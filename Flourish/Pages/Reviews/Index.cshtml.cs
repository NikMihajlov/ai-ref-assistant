using System.ComponentModel.DataAnnotations;
using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Reviews;

public class ReviewsIndexModel(
    CurrentUserService currentUserService,
    AppDbContext db,
    GoogleCalendarService calendarService,
    NotificationService notifications) : FlourishPageModel(currentUserService)
{
    public List<ReviewEvent> MyReviews { get; set; } = [];
    public bool CanSchedule { get; set; }
    public List<SelectListItem> EmployeeOptions { get; set; } = [];
    public List<SelectListItem> PeriodOptions { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public Guid RevieweeId { get; set; }
        [Required] public Guid ReviewPeriodId { get; set; }
        [Required] public DateTime ScheduledAt { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        CanSchedule = user.Role is UserRole.Manager or UserRole.TeamLead;

        MyReviews = await db.ReviewEvents
            .Include(re => re.Reviewer)
            .Include(re => re.ReviewPeriod)
            .Where(re => re.RevieweeId == user.Id && re.ScheduledAt >= DateTime.UtcNow)
            .OrderBy(re => re.ScheduledAt)
            .ToListAsync();

        if (CanSchedule) await LoadSelectsAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role is not (UserRole.Manager or UserRole.TeamLead)) return Forbid();

        if (!ModelState.IsValid)
        {
            CanSchedule = true;
            await LoadSelectsAsync(user);
            return Page();
        }

        var reviewee = await db.Users.FindAsync(Input.RevieweeId);
        var period = await db.ReviewPeriods.FindAsync(Input.ReviewPeriodId);
        var teamLead = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.TeamLead);

        if (reviewee is null || period is null || teamLead is null)
        {
            ModelState.AddModelError("", "Invalid selection.");
            CanSchedule = true;
            await LoadSelectsAsync(user);
            return Page();
        }

        var reviewEvent = new ReviewEvent
        {
            RevieweeId = reviewee.Id,
            ReviewerId = user.Id,
            ReviewPeriodId = period.Id,
            ScheduledAt = Input.ScheduledAt.ToUniversalTime(),
            ReviewPeriod = period
        };

        // Create Google Calendar event
        var (eventId, meetLink) = await calendarService.CreateReviewEventAsync(reviewEvent, reviewee, user, teamLead);
        reviewEvent.GoogleCalendarEventId = eventId;
        reviewEvent.MeetLink = meetLink;

        db.ReviewEvents.Add(reviewEvent);
        await db.SaveChangesAsync();

        // Notify reviewee
        await notifications.NotifyAsync(reviewee.Id, NotificationType.ReviewScheduled,
            $"Your review with {user.Name} is scheduled for {Input.ScheduledAt:MMM d, yyyy h:mm tt}.",
            "/Reviews");

        return RedirectToPage();
    }

    private async Task LoadSelectsAsync(User viewer)
    {
        var employees = viewer.Role == UserRole.TeamLead
            ? await db.Users.Where(u => u.Id != viewer.Id).OrderBy(u => u.Name).ToListAsync()
            : await db.Users.Where(u => u.ManagerId == viewer.Id).OrderBy(u => u.Name).ToListAsync();

        EmployeeOptions = employees.Select(u => new SelectListItem(u.Name, u.Id.ToString())).ToList();
        PeriodOptions = await db.ReviewPeriods
            .OrderByDescending(p => p.IsActive).ThenByDescending(p => p.StartDate)
            .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
            .ToListAsync();
    }
}
