using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Electric_Power_Monitoring_System.Services
{
    public class AbnormalConsumptionAlertService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AbnormalConsumptionAlertService> _logger;
        private readonly TimeZoneInfo _egyptTimeZone;

        public AbnormalConsumptionAlertService(IServiceScopeFactory scopeFactory, ILogger<AbnormalConsumptionAlertService> logger)
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
                 //var delay = nextMidnight - egyptNow;
                var delay = TimeSpan.FromSeconds(30); // TEMP - revert after testing

                _logger.LogInformation("Next abnormal consumption check scheduled at {NextMidnight} Egypt time", nextMidnight);

                await Task.Delay(delay, stoppingToken);

                await RunAlertCheck(stoppingToken);
            }
        }

        private async Task RunAlertCheck(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAbnormalConsumptionService>();

            try
            {
                await service.CheckAndProcessAllDevicesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in abnormal consumption alert check");
            }
        }
    }
}