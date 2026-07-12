namespace Electric_Power_Monitoring_System.DTOs
{
    public class AbnormalAlertNotificationDto
    {
        public string Title { get; set; } = "تنبيه من مبصر";
        public string Body { get; set; } = string.Empty;
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public decimal BaselineWh { get; set; }
        public decimal CurrentConsumptionWh { get; set; }
        public int Stage { get; set; }
        public List<string> Tips { get; set; } = new();
    }
}