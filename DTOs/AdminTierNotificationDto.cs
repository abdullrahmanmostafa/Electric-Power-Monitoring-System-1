namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminTierNotificationDto
    {
        public long Id { get; set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public decimal RemainingKWh { get; set; }
        public decimal NextTierPrice { get; set; }
        public string TipsJson { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}