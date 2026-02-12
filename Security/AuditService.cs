using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;

namespace _240519P_AS_ASSN2.Security
{
    public class AuditService
    {
        private readonly AppDbContext _db;

        public AuditService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string userId, string action, string? ip)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                IpAddress = ip
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
