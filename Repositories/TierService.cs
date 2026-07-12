using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;
using System.Text.Json;

namespace Electric_Power_Monitoring_System.Services
{
    public class TierService : ITierService
    {
        private readonly AppDbContext _context;
        private readonly IReadingRepository _readingRepo;
        private readonly IAiService _aiService;
        private readonly ILogger<TierService> _logger;

        public TierService(AppDbContext context, IReadingRepository readingRepo, IAiService aiService, ILogger<TierService> logger)
        {
            _context = context;
            _readingRepo = readingRepo;
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<TierStatusDto> GetUserTierStatusAsync(string userIdentifier)
        {
            // 1. حساب الاستهلاك التراكمي للشهر الحالي
            var (consumptionKWh, month, year) = await GetCurrentMonthConsumptionAsync(userIdentifier);

            // 2. تحديد الشريحة الحالية
            var tiers = await _context.TierSettings.Where(t => t.IsActive).OrderBy(t => t.MinKWh).ToListAsync();
            var currentTier = tiers.LastOrDefault(t => t.MinKWh <= consumptionKWh && (t.MaxKWh >= consumptionKWh || t.MaxKWh == decimal.MaxValue));

            if (currentTier == null)
                throw new Exception("No tier found for consumption");

            // 3. تحديد الشريحة التالية (إن وجدت)
            var nextTier = tiers.FirstOrDefault(t => t.MinKWh > consumptionKWh);
            decimal? remainingKWh = null;
            decimal? nextTierPrice = null;
            string? nextTierName = null;
            decimal? nextTierThreshold = null;
            bool isThresholdReached = false;
            List<string> tips = null;

            if (nextTier != null)
            {
                remainingKWh = nextTier.MinKWh - consumptionKWh;
                nextTierPrice = nextTier.PricePerKWh;
                nextTierName = nextTier.TierName;
                nextTierThreshold = nextTier.MinKWh;

                // تحقق إذا كان المتبقي <= 20 كيلووات
                if (remainingKWh <= 20)
                {
                    isThresholdReached = true;
                    // توليد النصائح (إذا كان المتبقي <= 20)
                    tips = await _aiService.GenerateTipsAsync(remainingKWh.Value, nextTierPrice.Value);
                    if (tips == null || tips.Count < 3)
                    {
                        // استخدام النصائح الاحتياطية
                        tips = await GetFallbackTips();
                    }
                }
            }

            return new TierStatusDto
            {
                CurrentConsumptionKWh = consumptionKWh,
                CurrentTierName = currentTier.TierName,
                CurrentTierPrice = currentTier.PricePerKWh,
                RemainingKWh = remainingKWh,
                NextTierName = nextTierName,
                NextTierPrice = nextTierPrice,
                NextTierThreshold = nextTierThreshold,
                IsThresholdReached = isThresholdReached,
                Tips = tips
            };
        }

        public async Task<bool> CheckAndSendAlertAsync(string userIdentifier)
        {
            var status = await GetUserTierStatusAsync(userIdentifier);
            if (!status.IsThresholdReached)
                return false; // لا حاجة لإرسال تنبيه

            // تأكد من عدم إرسال تنبيه مكرر لهذا الشهر
            var existing = await _context.TierNotifications
                .Where(n => n.UserIdentifier == userIdentifier && n.SentAt.Month == DateTime.UtcNow.Month && n.SentAt.Year == DateTime.UtcNow.Year)
                .FirstOrDefaultAsync();

            if (existing != null)
                return false; // تم الإرسال بالفعل هذا الشهر

            // حفظ التنبيه في قاعدة البيانات
            var notification = new TierNotification
            {
                UserIdentifier = userIdentifier,
                RemainingKWh = status.RemainingKWh.Value,
                NextTierPrice = status.NextTierPrice.Value,
                TipsJson = JsonSerializer.Serialize(status.Tips),
                SentAt = DateTime.UtcNow
            };
            _context.TierNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // إرسال إشعار FCM (سنقوم بتنفيذ ذلك في TierAlertService)
            // سنعيد true للإشارة إلى أنه تم التنبيه
            return true;
        }

        public async Task<List<TierSettingsDto>> GetTierSettingsAsync()
        {
            var tiers = await _context.TierSettings.OrderBy(t => t.MinKWh).ToListAsync();
            return tiers.Select(t => new TierSettingsDto
            {
                Id = t.Id,
                TierName = t.TierName,
                MinKWh = t.MinKWh,
                MaxKWh = t.MaxKWh,
                PricePerKWh = t.PricePerKWh,
                IsActive = t.IsActive
            }).ToList();
        }

        public async Task UpdateTierSettingsAsync(List<TierSettingsDto> settings)
        {
            foreach (var dto in settings)
            {
                var tier = await _context.TierSettings.FindAsync(dto.Id);
                if (tier != null)
                {
                    tier.TierName = dto.TierName;
                    tier.MinKWh = dto.MinKWh;
                    tier.MaxKWh = dto.MaxKWh;
                    tier.PricePerKWh = dto.PricePerKWh;
                    tier.IsActive = dto.IsActive;
                }
            }
            await _context.SaveChangesAsync();
        }

        // دوال مساعدة
        private async Task<(decimal ConsumptionKWh, int Month, int Year)> GetCurrentMonthConsumptionAsync(string userIdentifier)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1);
            var end = start.AddMonths(1);

            // الحصول على جميع أجهزة المستخدم (من UserHub)
            var hubSerials = await _context.UserHubs
                .Where(uh => uh.UserIdentifier == userIdentifier)
                .Select(uh => uh.HubSerial)
                .ToListAsync();

            decimal totalWh = 0;
            foreach (var serial in hubSerials)
            {
                var plugs = await _context.Plugs.Where(p => p.HubSerial == serial).ToListAsync();
                foreach (var plug in plugs)
                {
                    var consumption = await _readingRepo.GetAggregatedConsumptionAsync(serial, plug.PlugNumber, start, end);
                    totalWh += consumption;
                }
            }

            var consumptionKWh = totalWh / 1000; // تحويل وات/ساعة إلى كيلووات/ساعة
            return (consumptionKWh, now.Month, now.Year);
        }

        private async Task<List<string>> GetFallbackTips()
        {
            var tips = await _context.AiTipsCache.Where(t => t.IsActive).Select(t => t.TipText).ToListAsync();
            // إذا كان العدد أقل من 3، نكرر النصائح
            while (tips.Count < 3)
                tips.AddRange(tips);
            return tips.Take(3).ToList();
        }
    }
}