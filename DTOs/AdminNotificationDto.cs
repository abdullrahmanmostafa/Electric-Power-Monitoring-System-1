namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminNotificationDto
    {
        public long Id { get; set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public string? HubSerial { get; set; }
        public int? PlugNumber { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}