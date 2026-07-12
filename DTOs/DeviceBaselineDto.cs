namespace Electric_Power_Monitoring_System.DTOs
{
    public class DeviceBaselineDto
    {
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public decimal BaselineWh { get; set; }
        public DateTime CalculatedDate { get; set; }
    }
}