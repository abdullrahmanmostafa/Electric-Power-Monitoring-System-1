using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Repositories
{
    public interface IPlugRepository : IRepository<Plug>
    {
        Task<Plug?> GetByHubAndPlugNumberAsync(string hubSerial, int plugNumber);
        Task<IEnumerable<Plug>> GetPlugsByHubSerialAsync(string hubSerial);
        Task<Plug> AddOrUpdateAsync(Plug plug);
    }
}
