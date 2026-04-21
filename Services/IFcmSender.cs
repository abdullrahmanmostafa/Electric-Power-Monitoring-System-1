// IFcmSender.cs
namespace Electric_Power_Monitoring_System.Services
{
    public interface IFcmSender
    {
        Task<bool> SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
    }
}