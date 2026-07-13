using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Services
{
    public class AbnormalConsumptionService : IAbnormalConsumptionService
    {
        private readonly AppDbContext _context;
        private readonly IReadingRepository _readingRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserDeviceRepository _userDeviceRepo;
        private readonly IFcmSender _fcmSender;
        private readonly ILogger<AbnormalConsumptionService> _logger;

        public AbnormalConsumptionService(
            AppDbContext context,
            IReadingRepository readingRepo,
            INotificationRepository notificationRepo,
            IUserDeviceRepository userDeviceRepo,
            IFcmSender fcmSender,
            ILogger<AbnormalConsumptionService> logger)
        {
            _context = context;
            _readingRepo = readingRepo;
            _notificationRepo = notificationRepo;
            _userDeviceRepo = userDeviceRepo;
            _fcmSender = fcmSender;
            _logger = logger;
        }

        public async Task<List<DeviceStatusDto>> GetUserDevicesStatusAsync(string userIdentifier)
        {
            // جلب جميع الأجهزة (Hubs + Plugs) للمستخدم
            var hubSerials = await _context.UserHubs
                .Where(uh => uh.UserIdentifier == userIdentifier)
                .Select(uh => uh.HubSerial)
                .ToListAsync();

            var result = new List<DeviceStatusDto>();

            foreach (var serial in hubSerials)
            {
                var plugs = await _context.Plugs.Where(p => p.HubSerial == serial).ToListAsync();
                foreach (var plug in plugs)
                {
                    // جلب التتبع والخط الأساسي
                    var tracking = await _context.AbnormalConsumptionTrackings
                        .FirstOrDefaultAsync(t => t.HubSerial == serial && t.PlugNumber == plug.PlugNumber);

                    var baseline = await _context.DeviceBaselines
                        .FirstOrDefaultAsync(b => b.HubSerial == serial && b.PlugNumber == plug.PlugNumber);

                    // جلب استهلاك اليوم الحالي
                    var today = DateTime.UtcNow.Date;
                    var tomorrow = today.AddDays(1);
                    var currentConsumption = await _readingRepo.GetAggregatedConsumptionAsync(serial, plug.PlugNumber, today, tomorrow);

                    var baselineWh = baseline?.BaselineWh ?? 0;
                    var stage = tracking?.Stage ?? 0;
                    var isResolved = tracking?.IsResolved ?? false;

                    // جلب النصائح حسب المرحلة
                    var tips = await GetTipsForStage(stage);

                    result.Add(new DeviceStatusDto
                    {
                        HubSerial = serial,
                        PlugNumber = plug.PlugNumber,
                        BaselineWh = baselineWh,
                        CurrentDailyConsumptionWh = currentConsumption,
                        Stage = stage,
                        StageDescription = GetStageDescription(stage),
                        LastAlertDate = tracking?.LastAlertDate,
                        Tips = tips,
                        IsResolved = isResolved
                    });
                }
            }

            return result;
        }

        public async Task<DeviceBaselineDto?> GetDeviceBaselineAsync(string hubSerial, int plugNumber)
        {
            var baseline = await _context.DeviceBaselines
                .FirstOrDefaultAsync(b => b.HubSerial == hubSerial && b.PlugNumber == plugNumber);

            if (baseline == null) return null;

            return new DeviceBaselineDto
            {
                HubSerial = baseline.HubSerial,
                PlugNumber = baseline.PlugNumber,
                BaselineWh = baseline.BaselineWh,
                CalculatedDate = baseline.CalculatedDate
            };
        }

        public async Task CheckAndProcessAllDevicesAsync()
        {
            _logger.LogInformation("Starting abnormal consumption check for all devices");

            // جلب جميع الأجهزة الفريدة (HubSerial + PlugNumber) من جدول Plugs
            var allDevices = await _context.Plugs
                .Select(p => new { p.HubSerial, p.PlugNumber })
                .Distinct()
                .ToListAsync();

            foreach (var device in allDevices)
            {
                try
                {
                    await ProcessDeviceAsync(device.HubSerial, device.PlugNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing device {HubSerial}-{PlugNumber}", device.HubSerial, device.PlugNumber);
                }
            }

            _logger.LogInformation("Abnormal consumption check completed");
        }

        private async Task ProcessDeviceAsync(string hubSerial, int plugNumber)
        {
            // 1. حساب المعدل الطبيعي (Baseline)
            await CalculateBaselineAsync(hubSerial, plugNumber);

            // 2. جلب التتبع الحالي (إن وجد)
            var tracking = await _context.AbnormalConsumptionTrackings
                .FirstOrDefaultAsync(t => t.HubSerial == hubSerial && t.PlugNumber == plugNumber);

            // 3. جلب استهلاك اليوم الحالي
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var currentConsumption = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, today, tomorrow);

            // 4. جلب المعدل الطبيعي
            var baseline = await _context.DeviceBaselines
                .FirstOrDefaultAsync(b => b.HubSerial == hubSerial && b.PlugNumber == plugNumber);

            if (baseline == null) return;

            // 5. مقارنة الاستهلاك الحالي بالمعدل الطبيعي
            bool isAboveBaseline = currentConsumption > baseline.BaselineWh;

            // 6. تحديث التتبع بناءً على المقارنة
            if (!isAboveBaseline)
            {
                // الاستهلاك طبيعي -> إعادة الحالة إلى 0 (طبيعي)
                if (tracking != null)
                {
                    tracking.Stage = 0;
                    tracking.StageStartDate = null;
                    tracking.LastAlertDate = null;
                    tracking.IsResolved = true;
                    tracking.ResolvedDate = DateTime.UtcNow;
                    tracking.DailyConsumptionWh = currentConsumption;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // الاستهلاك أعلى من الطبيعي
            if (tracking == null)
            {
                // إنشاء سجل تتبع جديد
                tracking = new AbnormalConsumptionTracking
                {
                    HubSerial = hubSerial,
                    PlugNumber = plugNumber,
                    Stage = 1, // تنبيه أول
                    StageStartDate = DateTime.UtcNow,
                    LastAlertDate = DateTime.UtcNow,
                    DailyConsumptionWh = currentConsumption,
                    IsResolved = false
                };
                _context.AbnormalConsumptionTrackings.Add(tracking);
                await _context.SaveChangesAsync();
                await SendAlertAsync(hubSerial, plugNumber, baseline.BaselineWh, currentConsumption, 1);
                return;
            }

            // تحديث التتبع الحالي
            tracking.DailyConsumptionWh = currentConsumption;

            switch (tracking.Stage)
            {
                case 0: // طبيعي -> أصبح غير طبيعي
                    tracking.Stage = 1;
                    tracking.StageStartDate = DateTime.UtcNow;
                    tracking.LastAlertDate = DateTime.UtcNow;
                    tracking.IsResolved = false;
                    await _context.SaveChangesAsync();
                    await SendAlertAsync(hubSerial, plugNumber, baseline.BaselineWh, currentConsumption, 1);
                    break;

                case 1: // تنبيه أول -> انتظار 7 أيام
                    if (tracking.StageStartDate?.AddDays(7) <= DateTime.UtcNow)
                    {
                        tracking.Stage = 2;
                        tracking.StageStartDate = DateTime.UtcNow;
                        tracking.LastAlertDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await SendAlertAsync(hubSerial, plugNumber, baseline.BaselineWh, currentConsumption, 2);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                    break;

                case 2: // متابعة -> انتظار 7 أيام أخرى
                    if (tracking.StageStartDate?.AddDays(7) <= DateTime.UtcNow)
                    {
                        tracking.Stage = 3;
                        tracking.StageStartDate = DateTime.UtcNow;
                        tracking.LastAlertDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await SendAlertAsync(hubSerial, plugNumber, baseline.BaselineWh, currentConsumption, 3);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                    break;

                case 3: // فحص فني -> انتظار 3 أيام للتذكير
                case 4: // تذكير دوري
                    if (tracking.LastAlertDate?.AddDays(3) <= DateTime.UtcNow)
                    {
                        tracking.Stage = 4;
                        tracking.StageStartDate = tracking.StageStartDate ?? DateTime.UtcNow;
                        tracking.LastAlertDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await SendAlertAsync(hubSerial, plugNumber, baseline.BaselineWh, currentConsumption, 4);
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                    break;

                default:
                    await _context.SaveChangesAsync();
                    break;
            }
        }

        private async Task CalculateBaselineAsync(string hubSerial, int plugNumber)
        {
            // حساب متوسط آخر 7 أيام (عدا اليوم الحالي)
            var end = DateTime.UtcNow.Date;
            var start = end.AddDays(-7);

            var totalWh = 0m;
            var daysCount = 0;

            for (var day = start; day < end; day = day.AddDays(1))
            {
                var dayStart = day;
                var dayEnd = day.AddDays(1);
                var consumption = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, dayStart, dayEnd);
                if (consumption > 0)
                {
                    totalWh += consumption;
                    daysCount++;
                }
            }

            if (daysCount == 0) return;

            var averageWh = totalWh / daysCount;

            var existing = await _context.DeviceBaselines
                .FirstOrDefaultAsync(b => b.HubSerial == hubSerial && b.PlugNumber == plugNumber);

            if (existing == null)
            {
                var baseline = new DeviceBaseline
                {
                    HubSerial = hubSerial,
                    PlugNumber = plugNumber,
                    BaselineWh = averageWh,
                    CalculatedDate = DateTime.UtcNow
                };
                _context.DeviceBaselines.Add(baseline);
            }
            else
            {
                existing.BaselineWh = averageWh;
                existing.CalculatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private async Task SendAlertAsync(string hubSerial, int plugNumber, decimal baselineWh, decimal currentConsumption, int stage)
        {
            var tips = await GetTipsForStage(stage);

            var stageName = GetStageDescription(stage);
            var message = stage switch
            {
                1 => $"تم رصد استهلاك غير معتاد للجهاز {plugNumber} حيث يستهلك حالياً {currentConsumption / 1000:F1} كيلووات يومياً بينما معدله الطبيعي {baselineWh / 1000:F1} كيلووات. نصائح سريعة للفحص.",
                2 => $"استهلاك الجهاز {plugNumber} لا يزال أعلى من المعدل الطبيعي للأسبوع الثاني. إليك نصائح إضافية.",
                3 => $"استهلاك الجهاز {plugNumber} لا يزال أعلى من المعدل الطبيعي للأسبوع الثاني على التوالي. يرجى عمل فحص فني للجهاز.",
                4 => $"تذكير: استهلاك الجهاز {plugNumber} لا يزال مرتفعاً. يرجى عمل فحص فني لتجنب هدر الكهرباء.",
                _ => $"تنبيه: استهلاك الجهاز {plugNumber} غير طبيعي."
            };

            // جلب مستخدمي هذا الـ Hub
            var userIdentifiers = await _context.UserHubs
                .Where(uh => uh.HubSerial == hubSerial)
                .Select(uh => uh.UserIdentifier)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIdentifiers)
            {
                // جلب أجهزة المستخدم
                var devices = await _userDeviceRepo.GetByUserIdAsync(userId);
                foreach (var device in devices)
                {
                    if (device.FcmToken == "linked_hub") continue;
                    var body = $"{message}\n\n";
                    body += string.Join("\n", tips.Select((t, i) => $"{i + 1}- {t}"));
                    await _fcmSender.SendNotificationAsync(
                        device.FcmToken,
                        $"تنبيه من مبصر - {stageName}",
                        body
                    );
                }

                // تخزين الإشعار في قاعدة البيانات
                var notification = new Notification
                {
                    UserId = userId,
                    HubSerial = hubSerial,
                    PlugNumber = plugNumber,
                    Type = stage switch
                    {
                        1 => "first_alert",
                        2 => "follow_up_alert",
                        3 => "technical_inspection_alert",
                        4 => "reminder_alert",
                        _ => "abnormal_alert"
                    },
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    FcmResponse = "Sent"
                };
                await _notificationRepo.AddAsync(notification);
            }

            _logger.LogInformation("Abnormal alert (stage {Stage}) sent for device {HubSerial}-{PlugNumber}", stage, hubSerial, plugNumber);
        }

        private async Task<List<string>> GetTipsForStage(int stage)
        {
            var type = stage switch
            {
                1 => "abnormal_first",
                2 => "abnormal_followup",
                3 => "abnormal_inspection",
                4 => "abnormal_reminder",
                _ => "abnormal"
            };

            var tips = await _context.AiTipsCache
                .Where(t => t.Type == "abnormal" && t.IsActive)
                .Select(t => t.TipText)
                .ToListAsync();

            if (!tips.Any())
            {
                // نصائح احتياطية إذا كانت قاعدة البيانات فارغة
                tips = new List<string>
                {
                    "افحص الجهاز للتأكد من عدم وجود عطل.",
                    "تأكد من تنظيف الجهاز بانتظام.",
                    "راجع دليل المستخدم للصيانة الدورية."
                };
            }

            // اختيار 3 نصائح عشوائية
            var random = new Random();
            var selected = tips.OrderBy(x => random.Next()).Take(3).ToList();

            // إذا كان العدد أقل من 3، نكرر النصائح
            while (selected.Count < 3)
            {
                selected.AddRange(tips.Take(3 - selected.Count));
            }

            return selected;
        }

        private string GetStageDescription(int stage)
        {
            return stage switch
            {
                0 => "طبيعي",
                1 => "تنبيه أول",
                2 => "متابعة",
                3 => "فحص فني",
                4 => "تذكير دوري",
                _ => "غير معروف"
            };
        }
    }
}