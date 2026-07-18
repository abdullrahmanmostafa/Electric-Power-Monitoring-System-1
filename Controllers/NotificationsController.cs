// NotificationsController.cs
using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;
using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserDeviceRepository _userDeviceRepo;
        private readonly AppDbContext _context;

        public NotificationsController(INotificationRepository notificationRepo, IUserDeviceRepository userDeviceRepo, AppDbContext contex)
        {
            _notificationRepo = notificationRepo;
            _userDeviceRepo = userDeviceRepo;
            _context = contex;
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
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUnifiedNotifications()
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            var allNotifications = new List<UnifiedNotificationDto>();

            // 1. الإشعارات القديمة (من جدول Notifications)
            var oldNotifs = await _context.Notifications
                .Where(n => n.UserId == userIdentifier)
                .OrderByDescending(n => n.SentAt)
                .Select(n => new UnifiedNotificationDto
                {
                    Type = "old",
                    Title = "تنبيه عام",
                    Message = n.Message,
                    Timestamp = n.SentAt,
                    HubSerial = n.HubSerial,
                    PlugNumber = n.PlugNumber,
                    IsRead = false, // يمكن تعديله حسب الحاجة
                    NotificationId = n.Id
                })
                .ToListAsync();
            allNotifications.AddRange(oldNotifs);

            // 2. إشعارات الميزة الأولى (تجاوز الشرائح)
            var tierNotifs = await _context.TierNotifications
                .Where(t => t.UserIdentifier == userIdentifier)
                .OrderByDescending(t => t.SentAt)
                .Select(t => new UnifiedNotificationDto
                {
                    Type = "tier_alert",
                    Title = "تنبيه باقتراب شريحة جديدة",
                    Message = $"تبقى لك {t.RemainingKWh} كيلووات فقط، سعر الكيلووات الجديد {t.NextTierPrice} جنيه.",
                    Timestamp = t.SentAt,
                    RemainingKWh = t.RemainingKWh,
                    NextTierPrice = t.NextTierPrice,
                    NotificationId = t.Id
                })
                .ToListAsync();
            allNotifications.AddRange(tierNotifs);

            // 3. إشعارات الميزة الثانية (الاستهلاك غير الطبيعي)
            // ربط AbnormalConsumptionTracking مع UserHubs للحصول على UserIdentifier
            var abnormalNotifs = await _context.AbnormalConsumptionTrackings
                .Where(a => a.Stage > 0 && !a.IsResolved)
                .Join(_context.UserHubs,
                      a => a.HubSerial,
                      uh => uh.HubSerial,
                      (a, uh) => new { a, uh.UserIdentifier })
                .Where(x => x.UserIdentifier == userIdentifier)
                .Select(x => new UnifiedNotificationDto
                {
                    Type = "abnormal_alert",
                    Title = GetAbnormalTitle(x.a.Stage),
                    Message = GetAbnormalMessage(x.a.Stage, x.a.PlugNumber, x.a.DailyConsumptionWh ?? 0),
                    Timestamp = x.a.LastAlertDate ?? x.a.AlertStageStartDate,
                    HubSerial = x.a.HubSerial,
                    PlugNumber = x.a.PlugNumber,
                    Stage = x.a.Stage,
                    NotificationId = x.a.Id
                })
                .ToListAsync();
            allNotifications.AddRange(abnormalNotifs);

            // 4. إشعارات الميزة الثالثة (الإنارة المفقودة - حالة الإلزام وتأكيد التصحيح)
            // 4.1 حالة الإلزام (إذا كان مطلوباً التصوير)
            var mandatoryState = await _context.UserMandatoryStates
                .FirstOrDefaultAsync(m => m.UserIdentifier == userIdentifier);

            if (mandatoryState != null && mandatoryState.IsMandatory)
            {
                allNotifications.Add(new UnifiedNotificationDto
                {
                    Type = "mandatory_photo",
                    Title = "مطلوب تصوير عداد الكهرباء",
                    Message = $"يجب تصوير عداد الكهرباء قبل نهاية الشهر ({mandatoryState.ExpiryDate?.ToString("yyyy-MM-dd")})",
                    Timestamp = mandatoryState.LastUpdated,
                    IsMandatory = true,
                    NotificationId = mandatoryState.Id
                });
            }

            // 4.2 تأكيد التصحيح (آخر قراءة تم إدخالها)
            var lastCorrection = await _context.MeterReadings
                .Where(m => m.UserIdentifier == userIdentifier)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();

            if (lastCorrection != null)
            {
                allNotifications.Add(new UnifiedNotificationDto
                {
                    Type = "correction_confirmed",
                    Title = "تم تحديث قراءة العداد",
                    Message = $"تم تحديث قراءة العداد بتاريخ {lastCorrection.ReadingDate:yyyy-MM-dd} بقيمة {lastCorrection.ReadingValueWh} واط/ساعة.",
                    Timestamp = lastCorrection.ReadingDate,
                    NotificationId = lastCorrection.Id
                });
            }

            // 5. ترتيب النتائج تنازلياً حسب التاريخ (أحدثها أولاً)
            var sorted = allNotifications
                .OrderByDescending(n => n.Timestamp ?? DateTime.MinValue)
                .ToList();

            return Ok(sorted);
        }

        // دوال مساعدة للميزة الثانية (الاستهلاك غير الطبيعي)
        private string GetAbnormalTitle(int stage)
        {
            return stage switch
            {
                1 => "تنبيه: استهلاك غير طبيعي",
                2 => "متابعة: استهلاك غير طبيعي مستمر",
                3 => "تنبيه فني: استهلاك مرتفع جداً",
                4 => "تذكير: فحص الجهاز مطلوب",
                _ => "تنبيه استهلاك"
            };
        }

        private string GetAbnormalMessage(int stage, int plugNumber, decimal currentConsumption)
        {
            return stage switch
            {
                1 => $"تم رصد استهلاك غير معتاد للجهاز رقم {plugNumber} (الاستهلاك الحالي: {currentConsumption} واط/ساعة). يرجى الفحص.",
                2 => $"لا يزال استهلاك الجهاز رقم {plugNumber} مرتفعاً. يرجى متابعة الفحص.",
                3 => $"استهلاك الجهاز رقم {plugNumber} مرتفع جداً. يرجى طلب فحص فني فوري.",
                4 => $"تذكير: الجهاز رقم {plugNumber} لا يزال يستهلك بشكل غير طبيعي. يرجى التدخل.",
                _ => $"استهلاك غير طبيعي للجهاز رقم {plugNumber}."
            };
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