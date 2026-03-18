using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flourish.Pages.Account;

public class LoginModel : PageModel
{
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? error = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");

        if (error == "access_denied")
            ErrorMessage = "Access was denied. Please try again.";

        return Page();
    }
}
