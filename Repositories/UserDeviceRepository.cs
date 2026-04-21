// UserDeviceRepository.cs
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
{
    public UserDeviceRepository(AppDbContext context) : base(context) { }

    public async Task<UserDevice?> GetByUserIdAndTokenAsync(string userId, string token)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId && d.FcmToken == token);
    }

    public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(string userId)
    {
        return await _dbSet.Where(d => d.UserId == userId).ToListAsync();
    }
}