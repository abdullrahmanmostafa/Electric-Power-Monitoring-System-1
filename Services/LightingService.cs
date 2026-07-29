using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Services
{
    public class LightingService : ILightingService
    {
        private readonly AppDbContext _context;
        private readonly IReadingRepository _readingRepo;
        private readonly ILogger<LightingService> _logger;

        public LightingService(AppDbContext context, IReadingRepository readingRepo, ILogger<LightingService> logger)
        {
            _context = context;
            _readingRepo = readingRepo;
            _logger = logger;
        }

        public async Task ActivateMandatoryModeForUserAsync(string userIdentifier)
        {
            var state = await _context.UserMandatoryStates.FirstOrDefaultAsync(s => s.UserIdentifier == userIdentifier);
            var expiryDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));

            if (state == null)
            {
                state = new UserMandatoryState
                {
                    UserIdentifier = userIdentifier,
                    IsMandatory = true,
                    ExpiryDate = expiryDate,
                    LastUpdated = DateTime.UtcNow
                };
                _context.UserMandatoryStates.Add(state);
            }
            else
            {
                state.IsMandatory = true;
                state.ExpiryDate = expiryDate;
                state.LastUpdated = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SubmitMeterReadingAsync(string userIdentifier, MeterReadingRequestDto request)
        {
            var now = DateTime.UtcNow;
            var month = now.Month;
            var year = now.Year;

            // 1. حساب الاستهلاك المقاس بواسطة الأجهزة هذا الشهر
            var measuredWh = await GetMeasuredConsumptionAsync(userIdentifier, year, month);

            // 2. حساب استهلاك الإنارة الفعلي
            var actualLightingWh = request.ReadingValueWh - measuredWh;
            if (actualLightingWh < 0) actualLightingWh = 0;

            // 2a. حساب النسبة الفعلية للإنارة من إجمالي الاستهلاك المقاس
            var actualPercentage = measuredWh > 0 ? (actualLightingWh / measuredWh) * 100 : 0;

            // 3. تخزين قراءة العداد
            var meterReading = new MeterReading
            {
                UserIdentifier = userIdentifier,
                ReadingValueWh = request.ReadingValueWh,
                BalanceEgp = request.BalanceEgp,
                ReadingDate = now,
                Month = month,
                Year = year
            };
            _context.MeterReadings.Add(meterReading);

            // 4. تحديث جميع تقديرات الإنارة لهذا الشهر بالقيمة الفعلية والنسبة
            var estimates = await _context.LightingEstimates
                .Where(e => e.UserIdentifier == userIdentifier && e.Year == year && e.Month == month)
                .ToListAsync();

            if (estimates.Any())
            {
                var avgActual = actualLightingWh / estimates.Count;
                foreach (var estimate in estimates)
                {
                    estimate.ActualWh = avgActual;
                    estimate.ActualPercentage = actualPercentage; // تخزين النسبة الفعلية
                    estimate.IsCorrected = true;
                }
            }

            // 5. إلغاء وضع الإلزام للمستخدم
            await DeactivateMandatoryModeForUserAsync(userIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Meter reading submitted for user {UserId}, actual lighting: {ActualWh} Wh, percentage: {Percentage}%",
                userIdentifier, actualLightingWh, actualPercentage);
            return true;
        }

        public async Task<MeterStatusDto> GetMandatoryStatusAsync(string userIdentifier)
        {
            var state = await _context.UserMandatoryStates
                .FirstOrDefaultAsync(s => s.UserIdentifier == userIdentifier);

            if (state == null || !state.IsMandatory)
                return new MeterStatusDto { IsPhotoRequired = false, DaysRemaining = 0 };

            var daysRemaining = state.ExpiryDate.HasValue
                ? Math.Max(0, (int)(state.ExpiryDate.Value - DateTime.UtcNow.Date).TotalDays)
                : 0;

            return new MeterStatusDto
            {
                IsPhotoRequired = state.IsMandatory,
                ExpiryDate = state.ExpiryDate,
                DaysRemaining = daysRemaining
            };
        }

        public async Task<LightingConsumptionDto> GetLightingConsumptionAsync(string userIdentifier, int year, int month)
        {
            var estimates = await _context.LightingEstimates
                .Where(e => e.UserIdentifier == userIdentifier && e.Year == year && e.Month == month)
                .OrderBy(e => e.Day)
                .ToListAsync();

            var totalEstimated = estimates.Sum(e => e.EstimatedWh);
            var totalActual = estimates.Any(e => e.IsCorrected) ? estimates.Sum(e => e.ActualWh ?? 0) : (decimal?)null;
            var isCorrected = estimates.Any(e => e.IsCorrected);
            // استخراج النسبة الفعلية من أول سجل مصحح (جميع السجلات لها نفس النسبة بعد التصحيح)
            var actualPercentage = isCorrected ? estimates.FirstOrDefault(e => e.IsCorrected)?.ActualPercentage : null;

            var dailyData = estimates.Select(e => new DailyLightingDto
            {
                Day = e.Day,
                EstimatedWh = e.EstimatedWh,
                ActualWh = e.ActualWh,
                IsCorrected = e.IsCorrected
            }).ToList();

            return new LightingConsumptionDto
            {
                Year = year,
                Month = month,
                EstimatedTotalWh = totalEstimated,
                ActualTotalWh = totalActual,
                IsCorrected = isCorrected,
                EstimationPercentage = 13m, // نسبة التقدير الثابتة
                ActualPercentage = actualPercentage, // النسبة الفعلية بعد التصحيح (إن وجدت)
                DailyEstimates = dailyData
            };
        }

        public async Task ActivateMandatoryModeForAllUsersAsync()
        {
            var userIdentifiers = await _context.UserHubs
                .Select(uh => uh.UserIdentifier)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIdentifiers)
            {
                var state = await _context.UserMandatoryStates
                    .FirstOrDefaultAsync(s => s.UserIdentifier == userId);

                var expiryDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));

                if (state == null)
                {
                    state = new UserMandatoryState
                    {
                        UserIdentifier = userId,
                        IsMandatory = true,
                        ExpiryDate = expiryDate,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserMandatoryStates.Add(state);
                }
                else
                {
                    state.IsMandatory = true;
                    state.ExpiryDate = expiryDate;
                    state.LastUpdated = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Mandatory photo mode activated for all users");
        }

        public async Task DeactivateMandatoryModeForUserAsync(string userIdentifier)
        {
            var state = await _context.UserMandatoryStates
                .FirstOrDefaultAsync(s => s.UserIdentifier == userIdentifier);

            if (state != null)
            {
                state.IsMandatory = false;
                state.ExpiryDate = null;
                state.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // دالة مساعدة لحساب الاستهلاك المقاس بواسطة الأجهزة في الشهر
        private async Task<decimal> GetMeasuredConsumptionAsync(string userIdentifier, int year, int month)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

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

            return totalWh;
        }
    }
}