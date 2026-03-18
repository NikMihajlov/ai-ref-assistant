using System.ComponentModel.DataAnnotations;
using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Goals;

public class CreateGoalModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public List<SelectListItem> PeriodOptions { get; set; } = [];

    public class InputModel
    {
        [Required] public string Title { get; set; } = "";
        public string? Description { get; set; }
        [Required] public Guid ReviewPeriodId { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
        public bool IsPrivate { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadPeriodsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPeriodsAsync();
            return Page();
        }

        var user = await CurrentUserService.GetRequiredAsync();

        db.Goals.Add(new Goal
        {
            UserId = user.Id,
            ReviewPeriodId = Input.ReviewPeriodId,
            Title = Input.Title,
            Description = Input.Description,
            Status = Input.Status,
            IsPrivate = Input.IsPrivate
        });
        await db.SaveChangesAsync();

        return RedirectToPage("/Goals/Index");
    }

    private async Task LoadPeriodsAsync()
    {
        PeriodOptions = await db.ReviewPeriods
            .OrderByDescending(rp => rp.IsActive)
            .ThenByDescending(rp => rp.StartDate)
            .Select(rp => new SelectListItem(rp.Name, rp.Id.ToString()))
            .ToListAsync();
    }
}
