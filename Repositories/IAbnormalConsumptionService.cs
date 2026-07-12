using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Services
{
    public interface IAbnormalConsumptionService
    {
        Task<List<DeviceStatusDto>> GetUserDevicesStatusAsync(string userIdentifier);
        Task<DeviceBaselineDto?> GetDeviceBaselineAsync(string hubSerial, int plugNumber);
        Task CheckAndProcessAllDevicesAsync();
    }
}