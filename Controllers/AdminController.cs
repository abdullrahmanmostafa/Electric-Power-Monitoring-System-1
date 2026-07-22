using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;
using System.Diagnostics;
using Electric_Power_Monitoring_System.Services;
using Electric_Power_Monitoring_System.Models;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IReadingRepository _readingRepo;
        private readonly IConfiguration _config;
        private readonly ILightingService _lightingService;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public AdminController(
              AppDbContext context,
              IReadingRepository readingRepo,
              IConfiguration config,
              ILightingService lightingService)
        {
            _context = context;
            _readingRepo = readingRepo;
            _config = config;
            _lightingService = lightingService;
        }

        // ==============================
        // 1. Existing endpoints
        // ==============================

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

            var userHubs = _context.UserHubs.Where(uh => uh.UserIdentifier == user.UserIdentifier);
            _context.UserHubs.RemoveRange(userHubs);

            var userDevices = _context.UserDevices.Where(ud => ud.UserId == user.UserIdentifier);
            _context.UserDevices.RemoveRange(userDevices);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{email}' and all associated data deleted successfully." });
        }

        // GET /statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var totalUsers = await _context.Users.CountAsync();
            var totalHubs = await _context.Hubs.CountAsync();
            var totalPlugs = await _context.Plugs.CountAsync();
            var totalReadings = await _context.Readings.CountAsync();
            var totalNotifications = await _context.Notifications.CountAsync();

            var hubUserCounts = await _context.UserHubs
                .GroupBy(uh => uh.HubSerial)
                .Select(g => new { HubSerial = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.HubSerial, v => v.Count);

            return Ok(new AdminStatisticsDto
            {
                TotalUsers = totalUsers,
                TotalHubs = totalHubs,
                TotalPlugs = totalPlugs,
                TotalReadings = totalReadings,
                TotalNotifications = totalNotifications,
                HubUserCounts = hubUserCounts
            });
        }

        // GET /hubs
        [HttpGet("hubs")]
        public async Task<IActionResult> GetHubUserRelations()
        {
            if (!IsAdminRequest()) return Unauthorized();

            var hubs = await _context.UserHubs
                .GroupBy(uh => uh.HubSerial)
                .Select(g => new { Serial = g.Key, LinkedUsers = g.Count() })
                .ToListAsync();

            return Ok(hubs);
        }

        // GET /consumption/day
        [HttpGet("consumption/day")]
        public async Task<IActionResult> GetDayConsumptionAdmin(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime date)
        {
            if (!IsAdminRequest()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            var start = date.Date;
            var end = start.AddDays(1);
            var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

            return Ok(new
            {
                hubSerial,
                plugNumber,
                startDate = start,
                endDate = end,
                totalConsumptionWh = total,
                periodType = "day"
            });
        }

        // GET /consumption/week
        [HttpGet("consumption/week")]
        public async Task<IActionResult> GetWeekConsumptionAdmin(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime weekStart)
        {
            if (!IsAdminRequest()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            var start = weekStart.Date;
            var end = start.AddDays(7);
            var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

            return Ok(new
            {
                hubSerial,
                plugNumber,
                startDate = start,
                endDate = end,
                totalConsumptionWh = total,
                periodType = "week"
            });
        }

        // GET /consumption/month
        [HttpGet("consumption/month")]
        public async Task<IActionResult> GetMonthConsumptionAdmin(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            if (!IsAdminRequest()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");

            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

            return Ok(new
            {
                hubSerial,
                plugNumber,
                startDate = start,
                endDate = end,
                totalConsumptionWh = total,
                periodType = "month"
            });
        }

        // GET /consumption/hourly
        [HttpGet("consumption/hourly")]
        public async Task<IActionResult> GetHourlyConsumptionAdmin(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (!IsAdminRequest()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
            end = new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0, DateTimeKind.Utc);

            var periods = new List<object>();
            var current = start;

            while (current < end)
            {
                var periodEnd = current.AddHours(1);
                var consumption = await _readingRepo.GetHourlyConsumptionAsync(hubSerial, plugNumber, current, periodEnd);
                periods.Add(new
                {
                    start = current,
                    end = periodEnd,
                    consumptionWh = consumption
                });
                current = periodEnd;
            }

            return Ok(new
            {
                hubSerial,
                plugNumber,
                periods
            });
        }

        // GET /consumption/compare
        [HttpGet("consumption/compare")]
        public async Task<IActionResult> ComparePeriodsAdmin(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime period1Start,
            [FromQuery] DateTime period1End,
            [FromQuery] DateTime period2Start,
            [FromQuery] DateTime period2End)
        {
            if (!IsAdminRequest()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            var consumption1 = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, period1Start, period1End);
            var consumption2 = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, period2Start, period2End);

            decimal percentChange = 0;
            if (consumption2 != 0)
                percentChange = ((consumption1 - consumption2) / consumption2) * 100;
            else if (consumption1 != 0)
                percentChange = 100;

            var increase = consumption1 > consumption2;

            return Ok(new
            {
                consumptionPeriod1Wh = consumption1,
                consumptionPeriod2Wh = consumption2,
                percentChange = Math.Round(percentChange, 2),
                increase
            });
        }

        // GET /notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!IsAdminRequest()) return Unauthorized();

            var notifications = await _context.Notifications
                .OrderByDescending(n => n.SentAt)
                .Skip(offset)
                .Take(limit)
                .Select(n => new AdminNotificationDto
                {
                    Id = n.Id,
                    UserIdentifier = n.UserId,
                    HubSerial = n.HubSerial,
                    PlugNumber = n.PlugNumber,
                    Type = n.Type,
                    Message = n.Message,
                    SentAt = n.SentAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET /status
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            if (!IsAdminRequest()) return Unauthorized();

            var uptime = DateTime.UtcNow - _startTime;
            var memory = GC.GetTotalMemory(false) / (1024 * 1024);
            bool dbConnected = false;
            try
            {
                dbConnected = await _context.Database.CanConnectAsync();
            }
            catch { dbConnected = false; }

            return Ok(new AdminStatusDto
            {
                UptimeSeconds = uptime.TotalSeconds,
                MemoryUsageMB = memory,
                DatabaseConnected = dbConnected,
                ApiVersion = "1.0"
            });
        }

        // ==============================
        // 2. NEW endpoints for new features
        // ==============================

        // GET /tier/notifications – all tier notifications
        [HttpGet("tier/notifications")]
        public async Task<IActionResult> GetTierNotifications(
            [FromQuery] int? userId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var query = _context.TierNotifications
                .Join(_context.Users,
                      tn => tn.UserIdentifier,
                      u => u.UserIdentifier,
                      (tn, u) => new { tn, u })
                .AsQueryable();

            if (userId.HasValue)
            {
                var userIdentifier = await _context.Users
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.UserIdentifier)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(userIdentifier))
                    query = query.Where(x => x.tn.UserIdentifier == userIdentifier);
            }

            if (month.HasValue)
                query = query.Where(x => x.tn.SentAt.Month == month.Value);
            if (year.HasValue)
                query = query.Where(x => x.tn.SentAt.Year == year.Value);

            var result = await query
                .OrderByDescending(x => x.tn.SentAt)
                .Select(x => new AdminTierNotificationDto
                {
                    Id = x.tn.Id,
                    UserIdentifier = x.u.UserIdentifier,
                    UserName = x.u.FirstName + " " + x.u.LastName,
                    UserEmail = x.u.Email,
                    RemainingKWh = x.tn.RemainingKWh,
                    NextTierPrice = x.tn.NextTierPrice,
                    TipsJson = x.tn.TipsJson,
                    SentAt = x.tn.SentAt,
                    IsRead = x.tn.IsRead
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET /abnormal/devices – all abnormal devices
        [HttpGet("abnormal/devices")]
        public async Task<IActionResult> GetAbnormalDevices([FromQuery] int? stage)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var query = _context.AbnormalConsumptionTrackings
                .Where(a => a.Stage > 0 && !a.IsResolved)
                .Join(_context.DeviceBaselines,
                      a => new { a.HubSerial, a.PlugNumber },
                      b => new { b.HubSerial, b.PlugNumber },
                      (a, b) => new { a, b })
                .Join(_context.UserHubs,
                      ab => ab.a.HubSerial,
                      uh => uh.HubSerial,
                      (ab, uh) => new { ab.a, ab.b, uh.UserIdentifier })
                .Join(_context.Users,
                      x => x.UserIdentifier,
                      u => u.UserIdentifier,
                      (x, u) => new { x.a, x.b, x.UserIdentifier, u })
                .AsQueryable();

            if (stage.HasValue)
                query = query.Where(x => x.a.Stage == stage.Value);

            var result = await query
                .Select(x => new AdminAbnormalDeviceDto
                {
                    Id = x.a.Id,
                    HubSerial = x.a.HubSerial,
                    PlugNumber = x.a.PlugNumber,
                    UserIdentifier = x.u.UserIdentifier,
                    UserName = x.u.FirstName + " " + x.u.LastName,
                    UserEmail = x.u.Email,
                    BaselineWh = x.b.BaselineWh,
                    CurrentConsumptionWh = x.a.DailyConsumptionWh,
                    ExceedPercent = x.b.BaselineWh > 0
                        ? ((x.a.DailyConsumptionWh ?? 0) - x.b.BaselineWh) / x.b.BaselineWh * 100
                        : 0,
                    Stage = x.a.Stage,
                    StageName = GetStageName(x.a.Stage),
                    AlertStageStartDate = x.a.StageStartDate,
                    LastAlertDate = x.a.LastAlertDate,
                    DaysInStage = x.a.StageStartDate.HasValue
                        ? (int)(DateTime.UtcNow - x.a.StageStartDate.Value).TotalDays
                        : 0,
                    IsResolved = x.a.IsResolved
                })
                .ToListAsync();

            return Ok(result);
        }

        // POST /abnormal/devices/{id}/reset – manually reset an abnormal device
        [HttpPost("abnormal/devices/{id}/reset")]
        public async Task<IActionResult> ResetAbnormalDevice(int id)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var tracking = await _context.AbnormalConsumptionTrackings.FindAsync(id);
            if (tracking == null)
                return NotFound("Device not found");

            tracking.Stage = 0;
            tracking.IsResolved = true;
            tracking.ResolvedDate = DateTime.UtcNow;
            tracking.StageStartDate = null;
            tracking.LastAlertDate = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Device alerts reset successfully" });
        }

        // GET /lighting/estimates – all lighting estimates
        [HttpGet("lighting/estimates")]
        public async Task<IActionResult> GetLightingEstimates(
            [FromQuery] int? userId,
            [FromQuery] int? month,
            [FromQuery] int? year,
            [FromQuery] bool? isCorrected)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var query = _context.LightingEstimates
                .Join(_context.Users,
                      le => le.UserIdentifier,
                      u => u.UserIdentifier,
                      (le, u) => new { le, u })
                .AsQueryable();

            if (userId.HasValue)
            {
                var userIdentifier = await _context.Users
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.UserIdentifier)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(userIdentifier))
                    query = query.Where(x => x.le.UserIdentifier == userIdentifier);
            }

            if (month.HasValue)
                query = query.Where(x => x.le.Month == month.Value);
            if (year.HasValue)
                query = query.Where(x => x.le.Year == year.Value);
            if (isCorrected.HasValue)
                query = query.Where(x => x.le.IsCorrected == isCorrected.Value);

            var result = await query
                .GroupBy(x => new { x.le.UserIdentifier, x.le.Year, x.le.Month, x.u.FirstName, x.u.LastName, x.u.Email })
                .Select(g => new AdminLightingEstimateDto
                {
                    UserIdentifier = g.Key.UserIdentifier,
                    UserName = g.Key.FirstName + " " + g.Key.LastName,
                    UserEmail = g.Key.Email,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalEstimatedWh = g.Sum(x => x.le.EstimatedWh),
                    TotalActualWh = g.Any(x => x.le.IsCorrected) ? g.Sum(x => x.le.ActualWh ?? 0) : (decimal?)null,
                    IsCorrected = g.Any(x => x.le.IsCorrected),
                    CreatedAt = g.Max(x => x.le.CreatedAt)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToListAsync();

            return Ok(result);
        }

        // GET /lighting/meter-readings – all meter readings
        [HttpGet("lighting/meter-readings")]
        public async Task<IActionResult> GetMeterReadings(
            [FromQuery] int? userId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var query = _context.MeterReadings
                .Join(_context.Users,
                      mr => mr.UserIdentifier,
                      u => u.UserIdentifier,
                      (mr, u) => new { mr, u })
                .AsQueryable();

            if (userId.HasValue)
            {
                var userIdentifier = await _context.Users
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.UserIdentifier)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(userIdentifier))
                    query = query.Where(x => x.mr.UserIdentifier == userIdentifier);
            }

            if (month.HasValue)
                query = query.Where(x => x.mr.Month == month.Value);
            if (year.HasValue)
                query = query.Where(x => x.mr.Year == year.Value);

            var result = await query
                .OrderByDescending(x => x.mr.ReadingDate)
                .Select(x => new AdminMeterReadingDto
                {
                    Id = x.mr.Id,
                    UserIdentifier = x.u.UserIdentifier,
                    UserName = x.u.FirstName + " " + x.u.LastName,
                    UserEmail = x.u.Email,
                    ReadingValueWh = x.mr.ReadingValueWh,
                    BalanceEgp = x.mr.BalanceEgp,
                    ReadingDate = x.mr.ReadingDate,
                    Month = x.mr.Month,
                    Year = x.mr.Year
                })
                .ToListAsync();

            return Ok(result);
        }

        // POST /lighting/remind/{userId} – send reminder to user to take meter photo
        [HttpPost("lighting/remind/{userId}")]
        public async Task<IActionResult> SendLightingReminder(int userId)
        {
            if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            await _lightingService.ActivateMandatoryModeForUserAsync(user.UserIdentifier);

            return Ok(new { message = $"Reminder sent to user {user.FirstName} {user.LastName}" });
        }

        // ==============================
        // 3. Private helpers
        // ==============================

        private bool IsAdminRequest()
        {
            var adminKey = _config["Admin:ApiKey"];
            if (string.IsNullOrEmpty(adminKey)) return true;
            var providedKey = Request.Headers["X-Admin-Key"].FirstOrDefault();
            return providedKey == adminKey;
        }

        private string GetStageName(int stage)
        {
            return stage switch
            {
                1 => "تنبيه أول",
                2 => "متابعة أسبوعية",
                3 => "فحص فني",
                4 => "تذكير دوري",
                _ => "غير معروف"
            };
        }
    }
}