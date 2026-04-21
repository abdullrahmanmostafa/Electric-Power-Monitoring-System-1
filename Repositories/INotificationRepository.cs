using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(string userId, int limit = 50);
        Task<IEnumerable<Notification>> GetUnsentNotificationsAsync();
        Task DeleteAllForUserAsync(string userId);

    }
}
