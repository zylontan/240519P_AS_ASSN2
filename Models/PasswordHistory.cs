using System.ComponentModel.DataAnnotations;

namespace _240519P_AS_ASSN2.Models
{
    public class PasswordHistory
    {
        public int Id { get; set; }

        [Required]
        public string IdentityUserId { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
