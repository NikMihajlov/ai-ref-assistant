using System.Security.Claims;
using Flourish.Data;
using Flourish.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flourish.Pages.Account;

// This page is no longer used — the Google middleware handles /signin-google directly.
// Kept as a placeholder.
public class GoogleCallbackModel(AppDbContext db) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");
}
