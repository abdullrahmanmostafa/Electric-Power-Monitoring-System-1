// NotificationsController.cs
using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;
using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserDeviceRepository _userDeviceRepo;
        public NotificationsController(INotificationRepository notificationRepo, IUserDeviceRepository userDeviceRepo)
        {
            _notificationRepo = notificationRepo;
            _userDeviceRepo = userDeviceRepo;
        }
        [HttpPost("register-fcm")]
        public async Task<IActionResult> RegisterFcmToken([FromBody] RegisterFcmTokenDto request)
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User ID missing");

            if (string.IsNullOrWhiteSpace(request.FcmToken))
                return BadRequest("FCM token is required");

            // Check if token already exists for this user
            var existing = await _userDeviceRepo.GetByUserIdAndTokenAsync(userId, request.FcmToken);
            if (existing == null)
            {
                var device = new UserDevice
                {
                    UserId = userId,
                    FcmToken = request.FcmToken
                };
                await _userDeviceRepo.AddAsync(device);
            }
            else
            {
                existing.LastUpdated = DateTime.UtcNow;
                await _userDeviceRepo.UpdateAsync(existing);
            }

            return Ok(new { message = "FCM token registered" });
        }
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User ID missing");

            var notifications = await _notificationRepo.GetNotificationsByUserAsync(userId);
            var result = notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                SentAt = n.SentAt,
                HubSerial = n.HubSerial,
                PlugNumber = n.PlugNumber
            });

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User ID missing");

            await _notificationRepo.DeleteAllForUserAsync(userId);
            return Ok(new { message = "All notifications deleted" });
        }
    }
}