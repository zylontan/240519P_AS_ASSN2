using _240519P_AS_ASSN2.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;

namespace _240519P_AS_ASSN2.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
                return RedirectToPage("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);


            var resetLink = Url.Page(
                "/ResetPassword",
                null,
                new { email = Input.Email, token = encodedToken },
                Request.Scheme);

            await _emailService.SendEmailAsync(
                Input.Email,
                "Password Reset",
                $"Click <a href='{resetLink}'>here</a> to reset your password.");

            return RedirectToPage("ForgotPasswordConfirmation");
        }
    }
}
