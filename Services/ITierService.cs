using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Services
{
    public interface ITierService
    {
        Task<TierStatusDto> GetUserTierStatusAsync(string userIdentifier);
        Task<bool> CheckAndSendAlertAsync(string userIdentifier);
        Task<List<TierSettingsDto>> GetTierSettingsAsync();
        Task UpdateTierSettingsAsync(List<TierSettingsDto> settings);
    }
}