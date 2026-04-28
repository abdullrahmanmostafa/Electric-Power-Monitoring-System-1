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
            // 1. توحيد التوقيت إلى UTC
            if (start.Kind != DateTimeKind.Utc) start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            if (end.Kind != DateTimeKind.Utc) end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            // 2. جلب القراءات التي تقع بالكامل داخل الفترة [start, end]
            var readingsInRange = (await GetReadingsInRangeAsync(hubSerial, plugNumber, start, end))
                                  .OrderBy(r => r.Timestamp)
                                  .ToList();

            // 3. إذا لم توجد أي قراءة داخل الفترة → الاستهلاك صفر (لا حاجة لقراءات خارجية)
            if (!readingsInRange.Any())
                return 0;

            // 4. الحصول على القراءة التي تسبق بداية الفترة (إن وجدت)
            var beforeStart = await GetReadingBeforeTimestampAsync(hubSerial, plugNumber, start);
            // 5. الحصول على القراءة التي تلي نهاية الفترة (إن وجدت)
            var afterEnd = await GetReadingAfterTimestampAsync(hubSerial, plugNumber, end);

            // 6. تحديد طاقة البداية
            decimal startEnergy;
            if (beforeStart != null)
                startEnergy = beforeStart.CumulativeEnergyWh;
            else
                // حالة أول قراءة على الإطلاق: نستخدم أول قراءة داخل الفترة كبداية
                startEnergy = readingsInRange.First().CumulativeEnergyWh;

            // 7. تحديد طاقة النهاية
            decimal endEnergy;
            if (afterEnd != null)
                endEnergy = afterEnd.CumulativeEnergyWh;
            else
                // حالة آخر قراءة في الفترة: نستخدم آخر قراءة داخل الفترة كنهاية
                endEnergy = readingsInRange.Last().CumulativeEnergyWh;

            // 8. حساب الاستهلاك والتأكد من كونه غير سالب
            var consumption = endEnergy - startEnergy;
            return consumption < 0 ? -consumption : consumption;
        }
    }
}
