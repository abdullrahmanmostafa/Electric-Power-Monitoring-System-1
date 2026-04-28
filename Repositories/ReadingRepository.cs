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
        public async Task<Reading?> GetReadingNearTimestampAsync(string hubSerial, int plugNumber, DateTime timestamp, TimeSpan tolerance)
        {
            var lower = timestamp - tolerance;
            var upper = timestamp + tolerance;

            // Get readings within the tolerance window
            var candidates = await _dbSet
                .Where(r => r.HubSerial == hubSerial && r.PlugNumber == plugNumber && r.Timestamp >= lower && r.Timestamp <= upper)
                .ToListAsync();

            // Order by proximity to the target timestamp
            return candidates.OrderBy(r => Math.Abs((r.Timestamp - timestamp).Ticks)).FirstOrDefault();
        }
        public async Task<decimal> GetTotalConsumptionForDayAsync(string hubSerial, int plugNumber, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await GetConsumptionBetweenAsync(hubSerial, plugNumber, start, end);
        }

        public async Task<decimal> GetTotalConsumptionForWeekAsync(string hubSerial, int plugNumber, DateTime weekStartDate)
        {
            var start = weekStartDate.Date;
            var end = start.AddDays(7);
            return await GetConsumptionBetweenAsync(hubSerial, plugNumber, start, end);
        }

        public async Task<decimal> GetTotalConsumptionForMonthAsync(string hubSerial, int plugNumber, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            return await GetConsumptionBetweenAsync(hubSerial, plugNumber, start, end);
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
            // Ensure UTC kind
            if (start.Kind != DateTimeKind.Utc) start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            if (end.Kind != DateTimeKind.Utc) end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            // 1. Get readings strictly inside the period (for edge cases)
            var readingsInRange = (await GetReadingsInRangeAsync(hubSerial, plugNumber, start, end)).ToList();

            // 2. Determine start energy – nearest reading within ±30 minutes, otherwise 0
            var tolerance = TimeSpan.FromMinutes(30);
            var nearStart = await GetReadingNearTimestampAsync(hubSerial, plugNumber, start, tolerance);
            decimal startEnergy = nearStart?.CumulativeEnergyWh ?? 0;

            // 3. Determine end energy – latest reading before or at 'end'
            var beforeEnd = await GetReadingBeforeTimestampAsync(hubSerial, plugNumber, end);
            decimal endEnergy;
            if (beforeEnd != null)
            {
                endEnergy = beforeEnd.CumulativeEnergyWh;
            }
            else
            {
                // No reading before end: use the last reading inside the period if any, otherwise 0
                endEnergy = readingsInRange.LastOrDefault()?.CumulativeEnergyWh ?? 0;
            }

            // 4. Calculate consumption and make it non‑negative
            var consumption = endEnergy - startEnergy;
            return consumption < 0 ? -consumption : consumption;
        }
    }
}
