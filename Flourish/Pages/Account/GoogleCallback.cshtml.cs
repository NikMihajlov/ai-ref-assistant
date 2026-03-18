using System.Security.Claims;
using Flourish.Data;
using Flourish.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flourish.Pages.Account;

public class GoogleCallbackModel(AppDbContext db) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return RedirectToPage("/Account/Login", new { error = "access_denied" });

        var claims = result.Principal.Claims.ToList();
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? email;
        var avatar = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                  ?? result.Principal.FindFirstValue("picture");

        var user = db.Users.FirstOrDefault(u => u.GoogleId == googleId);
        if (user is null)
        {
            user = new User
            {
                GoogleId = googleId,
                Email = email,
                Name = name,
                AvatarUrl = avatar,
                Role = UserRole.Employee
            };
            db.Users.Add(user);
        }
        else
        {
            user.Name = name;
            user.AvatarUrl = avatar;
        }
        await db.SaveChangesAsync();

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, googleId));
        identity.AddClaim(new Claim(ClaimTypes.Name, name));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        if (!string.IsNullOrEmpty(avatar))
            identity.AddClaim(new Claim("picture", avatar));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return LocalRedirect(returnUrl ?? "/");
    }
}
