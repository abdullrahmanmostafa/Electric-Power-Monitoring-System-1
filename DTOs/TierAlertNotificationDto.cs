namespace Electric_Power_Monitoring_System.DTOs
{
    public class TierAlertNotificationDto
    {
        public string Title { get; set; } = "تنبيه من مبصر";
        public string Body { get; set; } = string.Empty;
        public decimal RemainingKWh { get; set; }
        public decimal NextTierPrice { get; set; }
        public List<string> Tips { get; set; } = new();
    }
}