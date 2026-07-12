namespace Electric_Power_Monitoring_System.DTOs
{
    public class DeviceStatusDto
    {
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public decimal BaselineWh { get; set; }
        public decimal CurrentDailyConsumptionWh { get; set; }
        public int Stage { get; set; }
        public string StageDescription { get; set; } = string.Empty;
        public DateTime? LastAlertDate { get; set; }
        public List<string>? Tips { get; set; }
        public bool IsResolved { get; set; }
    }
}