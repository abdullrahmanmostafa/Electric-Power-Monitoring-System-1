using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserInfoDto
                {
                    Id = u.Id,
                    UserIdentifier = u.UserIdentifier,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // DELETE api/admin/users/{email}
        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            // Find the user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = $"User with email '{email}' not found." });

            // Delete related data manually or rely on cascade
            // It's safer to delete in order to avoid foreign key conflicts.
            // First, find all hubs owned by this user
            var hubs = await _context.Hubs.Where(h => h.UserId == user.UserIdentifier).ToListAsync();
            foreach (var hub in hubs)
            {
                // Delete readings for this hub
                var readings = _context.Readings.Where(r => r.HubSerial == hub.Serial);
                _context.Readings.RemoveRange(readings);

                // Delete plugs for this hub
                var plugs = _context.Plugs.Where(p => p.HubSerial == hub.Serial);
                _context.Plugs.RemoveRange(plugs);

                // Delete notifications for this hub
                var notifications = _context.Notifications.Where(n => n.HubSerial == hub.Serial);
                _context.Notifications.RemoveRange(notifications);
            }
            _context.Hubs.RemoveRange(hubs);

            // Delete user devices (FCM tokens) for this user
            var userDevices = _context.UserDevices.Where(ud => ud.UserId == user.UserIdentifier);
            _context.UserDevices.RemoveRange(userDevices);

            // Finally delete the user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{email}' and all associated data deleted successfully." });
        }
    }
}