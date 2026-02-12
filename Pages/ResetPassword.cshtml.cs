using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace _240519P_AS_ASSN2.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;

        public ResetPasswordModel(
            UserManager<IdentityUser> userManager,
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
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Token { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string email, string token)
        {
            Input = new InputModel
            {
                Email = email,
                Token = token
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

            var decodedToken = System.Net.WebUtility.UrlDecode(Input.Token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                Input.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return Page();
            }

            // Update password age timestamp
            var profile = _db.Users
                .FirstOrDefault(u => u.IdentityUserId == user.Id);

            if (profile != null)
            {
                profile.LastPasswordChanged = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Password has been reset successfully.";

            return RedirectToPage("Login");
        }

    }
}
