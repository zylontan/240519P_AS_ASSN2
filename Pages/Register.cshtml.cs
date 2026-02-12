using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;
using _240519P_AS_ASSN2.Security;
using _240519P_AS_ASSN2.ViewModels;
using System.IO;
using System.Net;

namespace _240519P_AS_ASSN2.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly AuditService _audit;
        private readonly ReCaptchaService _recaptcha;

        public RegisterModel(
            AppDbContext db,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env,
            AuditService audit,
            ReCaptchaService recaptcha)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _audit = audit;
            _recaptcha = recaptcha;
        }


        [BindProperty]
        public Register RModel { get; set; } = new();

        [BindProperty]
        public string RecaptchaToken { get; set; } = "";

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var isHuman = await _recaptcha.VerifyTokenAsync(RecaptchaToken);

            if (!isHuman)
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed.");
                return Page();
            }

            if (_db.Users.Any(u => u.Email == RModel.Email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return Page();
            }

            if (RModel.ProfilePicture == null ||
                Path.GetExtension(RModel.ProfilePicture.FileName).ToLower() != ".jpg")
            {
                ModelState.AddModelError("", "Profile picture must be a .jpg file.");
                return Page();
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + ".jpg";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await RModel.ProfilePicture.CopyToAsync(stream);
            }

            var identityUser = new IdentityUser
            {
                UserName = RModel.Email,
                Email = RModel.Email
            };

            var result = await _userManager.CreateAsync(identityUser, RModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                _db.PasswordHistories.Add(new PasswordHistory
                {
                    IdentityUserId = identityUser.Id,
                    PasswordHash = identityUser.PasswordHash!
                });

                await _db.SaveChangesAsync();


                return Page();
            }

            // 🔥 CRITICAL: Link IdentityUserId
            var user = new User
            {
                IdentityUserId = identityUser.Id,
                FirstName = WebUtility.HtmlEncode(RModel.FirstName),
                LastName = WebUtility.HtmlEncode(RModel.LastName),
                Email = RModel.Email,
                MobileNo = WebUtility.HtmlEncode(RModel.MobileNo),
                BillingAddress = WebUtility.HtmlEncode(RModel.BillingAddress),
                ShippingAddress = WebUtility.HtmlEncode(RModel.ShippingAddress),
                EncryptedCreditCard = EncryptionService.Encrypt(RModel.CreditCard),
                ProfilePicturePath = "/uploads/" + fileName,
                LastPasswordChanged = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(
                identityUser.Id,
                "Registration",
                HttpContext.Connection.RemoteIpAddress?.ToString());


            return RedirectToPage("Login");
        }
    }
}
