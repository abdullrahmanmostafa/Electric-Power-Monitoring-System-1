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
    }
}
