namespace Electric_Power_Monitoring_System.DTOs
{
    public class LightingConsumptionDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal EstimatedTotalWh { get; set; }
        public decimal? ActualTotalWh { get; set; }
        public bool IsCorrected { get; set; }
        public List<DailyLightingDto> DailyEstimates { get; set; } = new();
    }

    public class DailyLightingDto
    {
        public int Day { get; set; }
        public decimal EstimatedWh { get; set; }
        public decimal? ActualWh { get; set; }
        public bool IsCorrected { get; set; }
    }
}