namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminLightingEstimateDto
    {
        public long Id { get; set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalEstimatedWh { get; set; }
        public decimal? TotalActualWh { get; set; }
        public bool IsCorrected { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}