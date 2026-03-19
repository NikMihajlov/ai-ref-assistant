using System.Security.Claims;
using Flourish.Data;
using Flourish.Models;
using Flourish.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
        options.Scope.Add("https://www.googleapis.com/auth/calendar");
        options.SaveTokens = true;

        var publicUrl = builder.Configuration["PublicUrl"]?.TrimEnd('/');
        options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
        {
            var redirect = ctx.RedirectUri;

            // Override redirect_uri when running behind a tunnel/proxy
            if (!string.IsNullOrEmpty(publicUrl))
                redirect = redirect.Replace(
                    Uri.EscapeDataString("http://localhost:5005/signin-google"),
                    Uri.EscapeDataString($"{publicUrl}/signin-google"));

            // Request offline access so Google returns a refresh token
            redirect += "&access_type=offline&prompt=consent";

            ctx.Response.Redirect(redirect);
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var principal = ctx.Principal!;
            var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var email    = principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var name     = principal.FindFirstValue(ClaimTypes.Name) ?? email;
            var avatar   = principal.FindFirstValue("urn:google:picture")
                        ?? principal.FindFirstValue("picture");

            var user = db.Users.FirstOrDefault(u => u.GoogleId == googleId);
            if (user is null)
            {
                user = new User { GoogleId = googleId, Email = email, Name = name, AvatarUrl = avatar };
                db.Users.Add(user);
            }
            else
            {
                user.Name = name;
                user.AvatarUrl = avatar;
            }
            await db.SaveChangesAsync();
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<GoogleCalendarService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

var app = builder.Build();

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
