namespace Electric_Power_Monitoring_System.DTOs
{
    public class CompareResponseDto
    {
        public decimal ConsumptionPeriod1Wh { get; set; }
        public decimal ConsumptionPeriod2Wh { get; set; }
        public decimal PercentChange { get; set; }
        public bool Increase { get; set; }
    }
}