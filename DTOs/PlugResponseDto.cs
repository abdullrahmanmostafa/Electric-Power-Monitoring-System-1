namespace Electric_Power_Monitoring_System.DTOs
{
    public class PlugResponseDto
    {
        public int PlugNumber { get; set; }
        public string? Name { get; set; }
        public decimal? LastEnergyWh { get; set; }
        public bool? LastState { get; set; }
        public DateTime? LastReadingTime { get; set; }
    }
}