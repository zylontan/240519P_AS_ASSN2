using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;
using _240519P_AS_ASSN2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _240519P_AS_ASSN2.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;

        public User? Profile { get; set; }
        public string? FullCreditCard { get; set; }

        public IndexModel(
            UserManager<IdentityUser> userManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);

            if (identityUser == null)
                return RedirectToPage("Login");

            Profile = _db.Users
                .FirstOrDefault(u => u.IdentityUserId == identityUser.Id);

            if (Profile == null)
                return RedirectToPage("Login");

            // 🔐 Enforce maximum password age (2 minutes for demo)
            if ((DateTime.UtcNow - Profile.LastPasswordChanged).TotalMinutes > 2)
            {
                return RedirectToPage("ChangePassword");
            }

            // Decrypt credit card for display
            FullCreditCard = EncryptionService
                .Decrypt(Profile.EncryptedCreditCard);

            return Page();
        }
    }
}
