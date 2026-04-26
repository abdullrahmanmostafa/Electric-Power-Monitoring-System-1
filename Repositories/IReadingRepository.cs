using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Repositories
{
    public interface IReadingRepository : IRepository<Reading>
    {
        Task<Reading?> GetLastReadingAsync(string hubSerial, int plugNumber);
        Task<Reading?> GetReadingBeforeTimestampAsync(string hubSerial, int plugNumber, DateTime timestamp);
        Task<Reading?> GetReadingAfterTimestampAsync(string hubSerial, int plugNumber, DateTime timestamp);
        Task<IEnumerable<Reading>> GetReadingsInRangeAsync(string hubSerial, int plugNumber, DateTime start, DateTime end);
        Task<decimal> GetConsumptionBetweenAsync(string hubSerial, int plugNumber, DateTime start, DateTime end);
        Task<decimal> GetTotalConsumptionForDayAsync(string hubSerial, int plugNumber, DateTime date);
        Task<decimal> GetTotalConsumptionForWeekAsync(string hubSerial, int plugNumber, DateTime weekStartDate); // weekStartDate = أول يوم في الأسبوع (الأحد)
        Task<decimal> GetTotalConsumptionForMonthAsync(string hubSerial, int plugNumber, int year, int month);
    }
}
