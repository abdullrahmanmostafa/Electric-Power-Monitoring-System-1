using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric_Power_Monitoring_System.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(string userId, int limit = 50)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnsentNotificationsAsync()
        {
            // For retry mechanism: notifications that failed to send (FCM response indicates error)
            // Or we can add a 'Status' column. For simplicity, we'll return all.
            // This can be enhanced later.
            return await _dbSet.ToListAsync();
        }
        public async Task DeleteAllForUserAsync(string userId)
        {
            var notifications = _dbSet.Where(n => n.UserId == userId);
            _dbSet.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
