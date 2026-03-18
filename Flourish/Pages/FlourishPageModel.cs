using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flourish.Pages;

public abstract class FlourishPageModel(CurrentUserService currentUserService) : PageModel
{
    protected CurrentUserService CurrentUserService { get; } = currentUserService;
    protected User? CurrentUser { get; private set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        CurrentUser = await CurrentUserService.GetAsync();
        if (CurrentUser is not null)
        {
            ViewData["AvatarUrl"] = CurrentUser.AvatarUrl;
            ViewData["UserRole"] = CurrentUser.Role.ToString();
            ViewData["IsManagerOrLead"] = CurrentUser.Role is UserRole.Manager or UserRole.TeamLead;
            ViewData["IsTeamLead"] = CurrentUser.Role == UserRole.TeamLead;
        }
        await next();
    }
}
