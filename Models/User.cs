using System.ComponentModel.DataAnnotations;

namespace _240519P_AS_ASSN2.Models
{
    public class User
    {
        public int Id { get; set; }

        // 🔥 Add this
        public string IdentityUserId { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string EncryptedCreditCard { get; set; } = string.Empty;

        [Required]
        public string MobileNo { get; set; } = string.Empty;

        public string BillingAddress { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime LastPasswordChanged { get; set; } = DateTime.UtcNow;

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        public string ProfilePicturePath { get; set; } = "";

        public string? ActiveSessionId { get; set; }

        public string? TwoFactorCode { get; set; }

        public DateTime? TwoFactorExpiry { get; set; }

    }
}
