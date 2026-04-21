using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric_Power_Monitoring_System.Repositories
{
    public class ReadingRepository : Repository<Reading>, IReadingRepository
    {
        public ReadingRepository(AppDbContext context) : base(context) { }

        public async Task<Reading?> GetLastReadingAsync(string hubSerial, int plugNumber)
        {
            return await _dbSet
                .Where(r => r.HubSerial == hubSerial && r.PlugNumber == plugNumber)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<Reading?> GetReadingBeforeTimestampAsync(string hubSerial, int plugNumber, DateTime timestamp)
        {
            return await _dbSet
                .Where(r => r.HubSerial == hubSerial && r.PlugNumber == plugNumber && r.Timestamp <= timestamp)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<Reading?> GetReadingAfterTimestampAsync(string hubSerial, int plugNumber, DateTime timestamp)
        {
            return await _dbSet
                .Where(r => r.HubSerial == hubSerial && r.PlugNumber == plugNumber && r.Timestamp >= timestamp)
                .OrderBy(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Reading>> GetReadingsInRangeAsync(string hubSerial, int plugNumber, DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(r => r.HubSerial == hubSerial && r.PlugNumber == plugNumber && r.Timestamp >= start && r.Timestamp <= end)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<decimal> GetConsumptionBetweenAsync(string hubSerial, int plugNumber, DateTime start, DateTime end)
        {
            var beforeStart = await GetReadingBeforeTimestampAsync(hubSerial, plugNumber, start);
            var afterEnd = await GetReadingAfterTimestampAsync(hubSerial, plugNumber, end);

            decimal? startEnergy = beforeStart?.CumulativeEnergyWh;
            decimal? endEnergy = afterEnd?.CumulativeEnergyWh;

            // If no reading before start, assume zero (or use first reading within range)
            if (startEnergy == null)
            {
                var firstInRange = (await GetReadingsInRangeAsync(hubSerial, plugNumber, start, end)).FirstOrDefault();
                startEnergy = firstInRange?.CumulativeEnergyWh ?? 0;
            }

            if (endEnergy == null)
            {
                var lastInRange = (await GetReadingsInRangeAsync(hubSerial, plugNumber, start, end)).LastOrDefault();
                endEnergy = lastInRange?.CumulativeEnergyWh ?? startEnergy;
            }

            return (endEnergy ?? 0) - (startEnergy ?? 0);
        }
    }
}
