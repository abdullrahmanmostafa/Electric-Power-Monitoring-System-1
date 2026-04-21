// NotificationResponseDto.cs
namespace Electric_Power_Monitoring_System.DTOs
{
    public class NotificationResponseDto
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string? HubSerial { get; set; }
        public int? PlugNumber { get; set; }
    }
}