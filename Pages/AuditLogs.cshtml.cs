using _240519P_AS_ASSN2.Data;
using _240519P_AS_ASSN2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _240519P_AS_ASSN2.Pages
{
    [Authorize]
    public class AuditLogsModel : PageModel
    {
        private readonly AppDbContext _db;

        public AuditLogsModel(AppDbContext db)
        {
            _db = db;
        }

        public List<AuditLog> Logs { get; set; } = new();

        public async Task OnGetAsync()
        {
            Logs = await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
