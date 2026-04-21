using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric_Power_Monitoring_System.Repositories
{
    public class PlugRepository : Repository<Plug>, IPlugRepository
    {
        public PlugRepository(AppDbContext context) : base(context) { }

        public async Task<Plug?> GetByHubAndPlugNumberAsync(string hubSerial, int plugNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.HubSerial == hubSerial && p.PlugNumber == plugNumber);
        }

        public async Task<IEnumerable<Plug>> GetPlugsByHubSerialAsync(string hubSerial)
        {
            return await _dbSet.Where(p => p.HubSerial == hubSerial).ToListAsync();
        }

        public async Task<Plug> AddOrUpdateAsync(Plug plug)
        {
            var existing = await GetByHubAndPlugNumberAsync(plug.HubSerial, plug.PlugNumber);
            if (existing != null)
            {
                // Update existing
                existing.Name = plug.Name ?? existing.Name;
                await UpdateAsync(existing);
                return existing;
            }
            else
            {
                // Add new
                return await AddAsync(plug);
            }
        }
    }
}
