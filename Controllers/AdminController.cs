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
                .ToListAsync();

            var result = new List<UserInfoDto>();
            foreach (var user in users)
            {
                // Find all hubs linked to this user (using UserIdentifier)
                var hubSerials = await _context.UserHubs
    .Where(uh => uh.UserIdentifier == user.UserIdentifier)
    .Select(uh => uh.HubSerial)
    .ToListAsync();

                result.Add(new UserInfoDto
                {
                    Id = user.Id,
                    UserIdentifier = user.UserIdentifier,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt,
                    HubSerials = hubSerials
                });
            }

            return Ok(result);
        }

        // DELETE api/admin/users/{email}
        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = $"User with email '{email}' not found." });

            // Delete related data
            var userHubs = _context.UserHubs.Where(uh => uh.UserIdentifier == user.UserIdentifier);
       
            _context.UserHubs.RemoveRange(userHubs);

            var userDevices = _context.UserDevices.Where(ud => ud.UserId == user.UserIdentifier);
            _context.UserDevices.RemoveRange(userDevices);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{email}' and all associated data deleted successfully." });
        }
    }
}