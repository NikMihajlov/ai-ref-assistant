using System.ComponentModel.DataAnnotations;
using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Periods;

public class PeriodsIndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<ReviewPeriod> Periods { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Name { get; set; } = "";
        [Required] public DateOnly StartDate { get; set; }
        [Required] public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role != UserRole.TeamLead) return Forbid();

        Periods = await db.ReviewPeriods.OrderByDescending(rp => rp.StartDate).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role != UserRole.TeamLead) return Forbid();

        if (!ModelState.IsValid)
        {
            Periods = await db.ReviewPeriods.OrderByDescending(rp => rp.StartDate).ToListAsync();
            return Page();
        }

        if (Input.IsActive)
            await db.ReviewPeriods.Where(p => p.IsActive).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false));

        db.ReviewPeriods.Add(new ReviewPeriod
        {
            Name = Input.Name,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            IsActive = Input.IsActive
        });
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetActiveAsync(Guid id)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role != UserRole.TeamLead) return Forbid();

        await db.ReviewPeriods.ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false));
        await db.ReviewPeriods.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, true));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        if (user.Role != UserRole.TeamLead) return Forbid();

        await db.ReviewPeriods.Where(p => p.Id == id).ExecuteDeleteAsync();
        return RedirectToPage();
    }
}
