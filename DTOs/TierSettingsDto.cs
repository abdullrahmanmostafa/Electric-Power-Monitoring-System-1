namespace Electric_Power_Monitoring_System.DTOs
{
    public class TierSettingsDto
    {
        public int Id { get; set; }
        public string TierName { get; set; } = string.Empty;
        public decimal MinKWh { get; set; }
        public decimal MaxKWh { get; set; }
        public decimal PricePerKWh { get; set; }
        public bool IsActive { get; set; }
    }
}