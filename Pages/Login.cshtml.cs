using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace _240519P_AS_ASSN2.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;
        private readonly AuditService _audit;
        private readonly EmailService _emailService;
        private readonly ReCaptchaService _recaptcha;


        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            AppDbContext db,
            AuditService audit,
            EmailService emailService,
            ReCaptchaService recaptcha)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _audit = audit;
            _emailService = emailService;
            _recaptcha = recaptcha;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        [BindProperty]
        public string RecaptchaToken { get; set; } = "";


        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var isHuman = await _recaptcha.VerifyTokenAsync(RecaptchaToken);

            Console.WriteLine("Recaptcha token: " + RecaptchaToken);

            if (!isHuman)
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed.");
                return Page();
            }

            // Script injection protection
            if (InputSanitizer.ContainsMaliciousScript(
                Input.Email,
                Input.Password))
            {
                ModelState.AddModelError("", "Invalid input detected.");
                return Page();
            }

            var identityUser = await _userManager.FindByEmailAsync(Input.Email);

            if (identityUser == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return Page();
            }

            if (await _userManager.IsLockedOutAsync(identityUser))
            {
                ModelState.AddModelError("", "Account locked. Try again in 1 minute.");
                return Page();
            }

            // Check password only (NO cookie yet)
            var result = await _signInManager.CheckPasswordSignInAsync(
                identityUser,
                Input.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Account locked due to multiple failed attempts.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    await _audit.LogAsync(
                        identityUser.Id,
                        "Login Failed",
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                }

                return Page();
            }

            // 🔐 PASSWORD CORRECT → START 2FA

            var profile = _db.Users
                .FirstOrDefault(u => u.IdentityUserId == identityUser.Id);

            if (profile == null)
            {
                ModelState.AddModelError("", "User profile not found.");
                return Page();
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            profile.TwoFactorCode = otp;
            profile.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(2);

            await _db.SaveChangesAsync();

            // Send OTP email
            await _emailService.SendEmailAsync(
                identityUser.Email!,
                "Your 2FA Code",
                $"Your OTP is: <b>{otp}</b><br/>Expires in 2 minutes."
            );

            // Store userId temporarily
            TempData["2FAUserId"] = identityUser.Id;

            await _audit.LogAsync(
                identityUser.Id,
                "Password Verified - OTP Sent",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return RedirectToPage("VerifyOTP");
        }

        public class LoginInputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
