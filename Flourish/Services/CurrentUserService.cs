using System.Security.Claims;
using Flourish.Data;
using Flourish.Models;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext db)
{
    public async Task<User?> GetAsync()
    {
        var googleId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (googleId is null) return null;
        return await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<User> GetRequiredAsync()
    {
        var user = await GetAsync();
        return user ?? throw new InvalidOperationException("User not found.");
    }

    public bool CanViewGoal(User viewer, Goal goal)
    {
        if (viewer.Role == UserRole.TeamLead) return true;
        if (goal.UserId == viewer.Id) return true;
        if (!goal.IsPrivate && viewer.Role == UserRole.Manager) return true;
        if (goal.IsPrivate && viewer.Role == UserRole.Manager)
        {
            // Manager can see private goals of direct reports
            return goal.User?.ManagerId == viewer.Id;
        }
        return !goal.IsPrivate;
    }
}
