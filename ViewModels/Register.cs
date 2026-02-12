using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace _240519P_AS_ASSN2.ViewModels
{
    public class Register
    {
        [Required]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First name must contain letters only.")]
        public string FirstName { get; set; } = "";

        [Required]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Last name must contain letters only.")]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [RegularExpression(@"^[689]\d{7}$", ErrorMessage = "Phone must be 8 digits and start with 6, 8, or 9.")]
        public string MobileNo { get; set; } = "";

        [Required]
        public string BillingAddress { get; set; } = "";

        [Required]
        public string ShippingAddress { get; set; } = "";

        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit card must be 16 digits.")]
        public string CreditCard { get; set; } = "";

        [Required]
        public IFormFile? ProfilePicture { get; set; }

        public bool ContainsScript()
        {
            var pattern = @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>";
            var combined = FirstName + LastName + Email +
                           BillingAddress + ShippingAddress;

            return Regex.IsMatch(combined, pattern, RegexOptions.IgnoreCase);
        }
    }
}
