namespace Electric_Power_Monitoring_System.DTOs
{
    public class ConsumptionPeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal ConsumptionWh { get; set; }
    }

    public class ConsumptionResponseDto
    {
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public List<ConsumptionPeriodDto> Periods { get; set; } = new();
    }
}