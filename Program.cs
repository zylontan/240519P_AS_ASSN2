using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<ReCaptchaService>();



// ----------------------
// DATABASE CONTEXTS
// ----------------------
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------
// IDENTITY CONFIG
// ----------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    // Lockout rules (DEMO)
    options.Lockout.MaxFailedAccessAttempts = 2;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// ----------------------
// COOKIE + SINGLE SESSION VALIDATION
// ----------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.LoginPath = "/Login";

    options.Events.OnValidatePrincipal = async context =>
    {
        Console.WriteLine("VALIDATION EXECUTED");

        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionClaim = context.Principal.FindFirst("SessionId")?.Value;

        Console.WriteLine("UserId claim: " + userId);
        Console.WriteLine("Session claim: " + sessionClaim);

        var profile = db.Users.FirstOrDefault(u => u.IdentityUserId == userId);

        if (profile == null)
        {
            Console.WriteLine("PROFILE NULL");
            return;
        }

        Console.WriteLine("DB Session: " + profile.ActiveSessionId);

        if (profile.ActiveSessionId != sessionClaim)
        {
            Console.WriteLine("MISMATCH — SIGNING OUT");
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    };
});


// ----------------------
builder.Services.AddRazorPages();

var app = builder.Build();

// ----------------------
// MIDDLEWARE PIPELINE
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/404");
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(context =>
{
    if (context.HttpContext.Response.StatusCode == 403)
        context.HttpContext.Response.Redirect("/403");

    if (context.HttpContext.Response.StatusCode == 404)
        context.HttpContext.Response.Redirect("/404");

    return Task.CompletedTask;
});


app.MapRazorPages();

app.Run();
