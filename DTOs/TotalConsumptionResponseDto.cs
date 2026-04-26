namespace Electric_Power_Monitoring_System.DTOs
{
    public class TotalConsumptionResponseDto
    {
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalConsumptionWh { get; set; }
        public string PeriodType { get; set; } = string.Empty; // "day", "week", "month"
    }
}