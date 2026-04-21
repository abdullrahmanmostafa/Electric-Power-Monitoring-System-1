using Electric_Power_Monitoring_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Electric_Power_Monitoring_System.Repositories
{
    public interface IUserDeviceRepository : IRepository<UserDevice>
    {
        Task<UserDevice?> GetByUserIdAndTokenAsync(string userId, string token);
        Task<IEnumerable<UserDevice>> GetByUserIdAsync(string userId);
    }
}