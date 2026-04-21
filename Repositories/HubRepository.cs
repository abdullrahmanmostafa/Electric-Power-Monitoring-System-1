using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric_Power_Monitoring_System.Repositories
{
    public class HubRepository : Repository<Hub>, IHubRepository
    {
        public HubRepository(AppDbContext context) : base(context) { }

        public async Task<Hub?> GetBySerialAsync(string serial)
        {
            return await _dbSet.FirstOrDefaultAsync(h => h.Serial == serial);
        }

        public async Task<IEnumerable<Hub>> GetHubsByUserIdAsync(string userId)
        {
            return await _dbSet.Where(h => h.UserId == userId).ToListAsync();
        }

        public async Task UpdateLastSeenAsync(string serial)
        {
            var hub = await GetBySerialAsync(serial);
            if (hub != null)
            {
                hub.LastSeen = DateTime.UtcNow;
                await UpdateAsync(hub);
            }
        }
    }
}
