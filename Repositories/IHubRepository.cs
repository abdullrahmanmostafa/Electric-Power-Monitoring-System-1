using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Repositories
{
    public interface IHubRepository : IRepository<Hub>
    {
        Task<Hub?> GetBySerialAsync(string serial);
        Task<IEnumerable<Hub>> GetHubsByUserIdAsync(string userId);
        Task UpdateLastSeenAsync(string serial);
    }
}
