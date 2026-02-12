using _240519P_AS_ASSN2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace _240519P_AS_ASSN2.Pages
{
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _db;

        public VerifyOTPModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(6)]
            public string OTP { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (!TempData.ContainsKey("2FAUserId"))
                return RedirectToPage("Login");

            TempData.Keep("2FAUserId");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!TempData.ContainsKey("2FAUserId"))
                return RedirectToPage("Login");

            var userId = TempData["2FAUserId"]?.ToString();
            TempData.Keep("2FAUserId");

            if (!ModelState.IsValid)
                return Page();

            var identityUser = await _userManager.FindByIdAsync(userId!);
            if (identityUser == null)
                return RedirectToPage("Login");

            var profile = _db.Users
                .FirstOrDefault(u => u.IdentityUserId == identityUser.Id);

            if (profile == null ||
                profile.TwoFactorCode != Input.OTP ||
                profile.TwoFactorExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return Page();
            }

            // Clear OTP
            profile.TwoFactorCode = null;
            profile.TwoFactorExpiry = null;

            // Generate SessionId
            var sessionId = Guid.NewGuid().ToString();
            profile.ActiveSessionId = sessionId;

            await _db.SaveChangesAsync();

            // Remove old SessionId claims
            var claims = await _userManager.GetClaimsAsync(identityUser);
            var oldSessionClaims = claims
                .Where(c => c.Type == "SessionId")
                .ToList();

            if (oldSessionClaims.Any())
                await _userManager.RemoveClaimsAsync(identityUser, oldSessionClaims);

            // Add new SessionId claim
            await _userManager.AddClaimAsync(identityUser,
                new Claim("SessionId", sessionId));

            // Issue cookie
            await _signInManager.SignInAsync(identityUser, false);

            return RedirectToPage("Index");
        }
    }
}
