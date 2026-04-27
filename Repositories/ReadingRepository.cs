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
            // تحويل التواريخ إلى UTC (لتجنب مشكلة Kind)
            if (start.Kind != DateTimeKind.Utc) start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            if (end.Kind != DateTimeKind.Utc) end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            // جلب القراءات داخل الفترة
            var readingsInRange = (await GetReadingsInRangeAsync(hubSerial, plugNumber, start, end)).ToList();

            // إذا لم توجد أي قراءة داخل الفترة → 0
            if (!readingsInRange.Any())
                return 0;

            // جلب القراءة قبل بداية الفترة (إن وجدت)
            var beforeStart = await GetReadingBeforeTimestampAsync(hubSerial, plugNumber, start);
            // جلب القراءة بعد نهاية الفترة (إن وجدت)
            var afterEnd = await GetReadingAfterTimestampAsync(hubSerial, plugNumber, end);

            decimal startEnergy = beforeStart?.CumulativeEnergyWh ?? 0;
            decimal endEnergy = afterEnd?.CumulativeEnergyWh ?? 0;

            // إذا كان afterEnd غير موجود، استخدم آخر قراءة داخل الفترة
            if (afterEnd == null)
                endEnergy = readingsInRange.Last().CumulativeEnergyWh;

            // إذا كان beforeStart غير موجود، استخدم أول قراءة داخل الفترة كبداية
            // لكن لا نستعمل 0 لأن ذلك سيُظهر استهلاك وهمي في الفترات السابقة.
            // في حالة عدم وجود beforeStart، نعتمد على that the consumption is simply endEnergy - firstInRange? 
            // لكن العبرة: نحن نعلم أن قبل أول قراءة كان الاستهلاك صفراً، وأول قراءة حدثت داخل الفترة الحالية (لأن readingsInRange not empty).
            // إذن استهلاك الفترة الحالية = (endEnergy - firstInRange) + (firstInRange - 0) = endEnergy.
            // لذلك إذا كان beforeStart == null، فالاستهلاك = endEnergy (لأنه فرق بين آخر قراءة وبين الصفر).
            // ولكن هذا سيعيد 100 للفترة التي تحتوي أول قراءة وهذا صحيح، لكنه سيعيد أيضاً 100 لأي فترة سابقة (وهي فارغة)؟ لا لأننا شرطنا أن readingsInRange موجودة فقط، لذلك لن يحدث ذلك.
            // فحالة قبل أول قراءة تكون readingsInRange فارغة، وبالتالي ترجع 0.
            // إذن الكود الحالي يعمل بشكل صحيح مع إضافة شرط !readingsInRange.Any().
            // لكن نحتاج إلى ضبط startEnergy و endEnergy بشكل دقيق.

            // نستخدم الخوارزمية الكلاسيكية:
            if (beforeStart != null && afterEnd != null)
                return afterEnd.CumulativeEnergyWh - beforeStart.CumulativeEnergyWh;

            if (beforeStart == null && afterEnd != null)
                return afterEnd.CumulativeEnergyWh; // لأن بداية التراكم من الصفر
                                                    // (هذه الحالة لن تحدث لأننا تحققنا من وجود readingsInRange، فعلاً قد تحدث إذا كان afterEnd من القراءات المستقبلية وليس هناك beforeStart)
                                                    // لكن نضمن صحة.

            // أسهل طريقة لإرضاء جميع الحالات:
            // استخدام القراءة الأولى داخل الفترة كـ startEnergy إذا لم نجد beforeStart.
            // واستخدام القراءة الأخيرة داخل الفترة كـ endEnergy إذا لم نجد afterEnd.
            var effectiveStartEnergy = beforeStart?.CumulativeEnergyWh ?? readingsInRange.First().CumulativeEnergyWh;
            var effectiveEndEnergy = afterEnd?.CumulativeEnergyWh ?? readingsInRange.Last().CumulativeEnergyWh;
            return effectiveEndEnergy - effectiveStartEnergy;
        }
    }
}
