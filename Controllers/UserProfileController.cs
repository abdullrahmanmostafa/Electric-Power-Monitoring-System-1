using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // جلب بيانات المستخدم (أول جهاز لهذا user_id)
        [HttpGet]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "X-User-Id")] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("X-User-Id header is required");

            var device = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (device == null)
                return NotFound("No profile found for this user");

            return Ok(new UserProfileDto
            {
                FirstName = device.FirstName,
                LastName = device.LastName,
                Email = device.Email,
                Phone = device.Phone,
                Password = device.Password
            });
        }

        // إنشاء أو تحديث بيانات المستخدم (لجميع أجهزته)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromHeader(Name = "X-User-Id")] string userId, [FromBody] UserProfileDto profile)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("X-User-Id header is required");

            // البحث عن جميع أجهزة هذا المستخدم
            var devices = await _context.UserDevices
                .Where(d => d.UserId == userId)
                .ToListAsync();

            if (!devices.Any())
                return NotFound("No devices found for this user. Please register FCM token first.");

            // تحديث جميع الأجهزة بنفس البيانات
            foreach (var device in devices)
            {
                device.FirstName = profile.FirstName;
                device.LastName = profile.LastName;
                device.Email = profile.Email;
                device.Phone = profile.Phone;
                device.Password = profile.Password; // تخزين كما هو
                device.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }

        // حذف البيانات الشخصية للمستخدم (من جميع أجهزته)
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile([FromHeader(Name = "X-User-Id")] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("X-User-Id header is required");

            var devices = await _context.UserDevices
                .Where(d => d.UserId == userId)
                .ToListAsync();

            if (!devices.Any())
                return NotFound("No profile found");

            foreach (var device in devices)
            {
                device.FirstName = null;
                device.LastName = null;
                device.Email = null;
                device.Phone = null;
                device.Password = null;
                device.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile data deleted" });
        }
    }
}