using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Pages.Whiteboard;

public record GoalNode(Guid Id, string Title, string Status, bool IsPrivate, DateOnly? DueDate, string OwnerId);
public record SharedGoalNode(Guid Id, string Title, string Status, string? Description, string? Notes, List<string> MemberIds);
public record UserNode(Guid Id, string Name, string? AvatarUrl, List<GoalNode> Goals);
public record LinkEdge(Guid Id, Guid GoalId1, Guid GoalId2, string? Note);

public class WhiteboardIndexModel(CurrentUserService currentUserService, AppDbContext db) : FlourishPageModel(currentUserService)
{
    public List<UserNode> Users { get; set; } = [];
    public List<LinkEdge> Links { get; set; } = [];
    public List<SharedGoalNode> SharedGoals { get; set; } = [];
    public List<ReviewPeriod> Periods { get; set; } = [];
    public Guid? SelectedPeriodId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? periodId)
    {
        var viewer = await CurrentUserService.GetRequiredAsync();

        Periods = await db.ReviewPeriods.OrderByDescending(p => p.StartDate).ToListAsync();
        SelectedPeriodId = periodId ?? Periods.FirstOrDefault(p => p.IsActive)?.Id ?? Periods.FirstOrDefault()?.Id;

        if (SelectedPeriodId is null) return Page();

        (Users, Links, SharedGoals) = await FetchDataAsync(viewer, SelectedPeriodId.Value);
        return Page();
    }

    public async Task<IActionResult> OnGetDataAsync(Guid periodId)
    {
        var viewer = await CurrentUserService.GetRequiredAsync();
        var (users, links, sharedGoals) = await FetchDataAsync(viewer, periodId);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        return new JsonResult(new { users, links, sharedGoals }, jsonOptions);
    }

    private async Task<(List<UserNode>, List<LinkEdge>, List<SharedGoalNode>)> FetchDataAsync(Models.User viewer, Guid periodId)
    {
        var allUsers = await db.Users.OrderBy(u => u.Name).ToListAsync();
        var allGoals = await db.Goals
            .Where(g => g.ReviewPeriodId == periodId)
            .Include(g => g.User)
            .ToListAsync();

        var users = allUsers.Select(u =>
        {
            var userGoals = allGoals
                .Where(g => g.UserId == u.Id && CurrentUserService.CanViewGoal(viewer, g))
                .Select(g => new GoalNode(g.Id, g.Title, g.Status.ToString(), g.IsPrivate, g.DueDate, u.Id.ToString()))
                .ToList();
            return new UserNode(u.Id, u.Name, u.AvatarUrl, userGoals);
        }).Where(u => u.Goals.Count > 0).ToList();

        var goalIds = allGoals.Select(g => g.Id).ToList();
        var links = await db.GoalLinks
            .Where(l => goalIds.Contains(l.GoalId1) && goalIds.Contains(l.GoalId2))
            .Select(l => new LinkEdge(l.Id, l.GoalId1, l.GoalId2, l.Note))
            .ToListAsync();

        var sharedGoals = await db.SharedGoals
            .Where(sg => sg.ReviewPeriodId == periodId)
            .Include(sg => sg.Members)
            .Select(sg => new SharedGoalNode(
                sg.Id, sg.Title, sg.Status.ToString(),
                sg.Description, sg.Notes,
                sg.Members.Select(m => m.UserId.ToString()).ToList()))
            .ToListAsync();

        return (users, links, sharedGoals);
    }

    // ── GoalLink handlers ─────────────────────────────────────────────────────

    public async Task<IActionResult> OnPostLinkAsync(Guid goalId1, Guid goalId2, string? note)
    {
        var user = await CurrentUserService.GetRequiredAsync();
        var a = goalId1 < goalId2 ? goalId1 : goalId2;
        var b = goalId1 < goalId2 ? goalId2 : goalId1;

        var existing = await db.GoalLinks.FirstOrDefaultAsync(l => l.GoalId1 == a && l.GoalId2 == b);
        if (existing is null)
        {
            var link = new GoalLink { GoalId1 = a, GoalId2 = b, Note = note, CreatedById = user.Id };
            db.GoalLinks.Add(link);
            await db.SaveChangesAsync();
            return new JsonResult(new { id = link.Id });
        }
        return new JsonResult(new { id = existing.Id });
    }

    public async Task<IActionResult> OnPostUnlinkAsync(Guid linkId)
    {
        var link = await db.GoalLinks.FindAsync(linkId);
        if (link is not null) { db.GoalLinks.Remove(link); await db.SaveChangesAsync(); }
        return new OkResult();
    }

    public async Task<IActionResult> OnPostUpdateLinkNoteAsync(Guid linkId, string? note)
    {
        var link = await db.GoalLinks.FindAsync(linkId);
        if (link is not null) { link.Note = note; await db.SaveChangesAsync(); }
        return new OkResult();
    }

    // ── SharedGoal handlers ───────────────────────────────────────────────────

    public async Task<IActionResult> OnPostCreateSharedGoalAsync(
        string title, string? description, string? notes, GoalStatus status,
        Guid periodId, string memberIds)
    {
        var user = await CurrentUserService.GetRequiredAsync();

        var sg = new SharedGoal
        {
            Title = title,
            Description = description,
            Notes = notes,
            Status = status,
            ReviewPeriodId = periodId,
            CreatedById = user.Id
        };
        db.SharedGoals.Add(sg);
        await db.SaveChangesAsync();

        var ids = memberIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue).Select(g => g!.Value).ToList();

        foreach (var uid in ids)
            db.SharedGoalMembers.Add(new SharedGoalMember { SharedGoalId = sg.Id, UserId = uid });

        await db.SaveChangesAsync();

        return new JsonResult(new
        {
            id = sg.Id,
            title = sg.Title,
            status = sg.Status.ToString(),
            description = sg.Description,
            notes = sg.Notes,
            memberIds = ids.Select(i => i.ToString()).ToList()
        });
    }

    public async Task<IActionResult> OnPostUpdateSharedGoalAsync(
        Guid id, string? title, string? description, string? notes, GoalStatus status)
    {
        var sg = await db.SharedGoals.FindAsync(id);
        if (sg is null) return NotFound();

        if (title is not null) sg.Title = title;
        sg.Description = description;
        sg.Notes = notes;
        sg.Status = status;
        sg.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new OkResult();
    }

    public async Task<IActionResult> OnPostDeleteSharedGoalAsync(Guid id)
    {
        var sg = await db.SharedGoals.FindAsync(id);
        if (sg is not null) { db.SharedGoals.Remove(sg); await db.SaveChangesAsync(); }
        return new OkResult();
    }
}
