using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Services
{
    public interface ILightingService
    {
        Task<bool> SubmitMeterReadingAsync(string userIdentifier, MeterReadingRequestDto request);
        Task<MeterStatusDto> GetMandatoryStatusAsync(string userIdentifier);
        Task<LightingConsumptionDto> GetLightingConsumptionAsync(string userIdentifier, int year, int month);
        Task ActivateMandatoryModeForAllUsersAsync();
        Task DeactivateMandatoryModeForUserAsync(string userIdentifier);
    }
}