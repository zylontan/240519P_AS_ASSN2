using System.ComponentModel.DataAnnotations;

namespace _240519P_AS_ASSN2.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }
    }
}
