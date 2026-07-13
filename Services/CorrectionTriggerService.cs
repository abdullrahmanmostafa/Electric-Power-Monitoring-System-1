using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Electric_Power_Monitoring_System.Services
{
    public class CorrectionTriggerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CorrectionTriggerService> _logger;
        private readonly TimeZoneInfo _egyptTimeZone;

        public CorrectionTriggerService(IServiceScopeFactory scopeFactory, ILogger<CorrectionTriggerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time") ?? TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var egyptNow = TimeZoneInfo.ConvertTime(now, _egyptTimeZone);
                var nextMidnight = egyptNow.Date.AddDays(1);
                var delay = nextMidnight - egyptNow;

                await Task.Delay(delay, stoppingToken);

                await RunCorrectionCheck(stoppingToken);
            }
        }

        private async Task RunCorrectionCheck(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var lightingService = scope.ServiceProvider.GetRequiredService<ILightingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CorrectionTriggerService>>();

            var now = DateTime.UtcNow;
            var day = now.Day;

            // تفعيل الإلزام في يوم 28 أو 29 من كل شهر
            if (day == 28 || day == 29)
            {
                await lightingService.ActivateMandatoryModeForAllUsersAsync();
                logger.LogInformation("Mandatory photo mode activated for all users on day {Day}", day);
            }
            else
            {
                logger.LogInformation("No correction trigger today (day {Day})", day);
            }
        }
    }
}