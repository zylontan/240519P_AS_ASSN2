using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace _240519P_AS_ASSN2.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser>
    _userManager;
        private readonly AppDbContext _db;

        public ChangePasswordModel(
        UserManager<IdentityUser>
            userManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult>
            OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("Login");

            var passwordCheck = await _userManager.CheckPasswordAsync(user, Input.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError("", "Current password incorrect.");
                return Page();
            }

            if (Input.CurrentPassword == Input.NewPassword)
            {
                ModelState.AddModelError("", "New password cannot be the same as current password.");
                return Page();
            }


            // 🔥 Check last 2 passwords
            var histories = _db.PasswordHistories
            .Where(p => p.IdentityUserId == user.Id)
            .OrderByDescending(p => p.ChangedAt)
            .Take(2)
            .ToList();

            var hasher = _userManager.PasswordHasher;

            foreach (var history in histories)
            {
                var result = hasher.VerifyHashedPassword(
                user,
                history.PasswordHash,
                Input.NewPassword);

                if (result == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError("", "You cannot reuse your last 2 passwords.");
                    return Page();
                }
            }

            var changeResult = await _userManager.ChangePasswordAsync(
            user,
            Input.CurrentPassword,
            Input.NewPassword);

            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return Page();
            }

            var profile = _db.Users
                .FirstOrDefault(u => u.IdentityUserId == user.Id);

            if (profile != null)
            {
                profile.LastPasswordChanged = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }


            // Store new password in history
            _db.PasswordHistories.Add(new PasswordHistory
            {
                IdentityUserId = user.Id,
                PasswordHash = user.PasswordHash!
            });

            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
