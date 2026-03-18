using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flourish.Pages.Account;

public class GoogleLoginModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null)
    {
        var redirectUrl = Url.Page("/Account/GoogleCallback", values: new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
}
