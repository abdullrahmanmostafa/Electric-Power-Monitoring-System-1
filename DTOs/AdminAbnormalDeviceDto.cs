namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminAbnormalDeviceDto
    {
        public long Id { get; set; }
        public string HubSerial { get; set; } = string.Empty;
        public int PlugNumber { get; set; }
        public string? UserIdentifier { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public decimal BaselineWh { get; set; }
        public decimal? CurrentConsumptionWh { get; set; }
        public decimal ExceedPercent { get; set; }
        public int Stage { get; set; }
        public string StageName { get; set; } = string.Empty;
        public DateTime? AlertStageStartDate { get; set; }
        public DateTime? LastAlertDate { get; set; }
        public int DaysInStage { get; set; }
        public bool IsResolved { get; set; }
    }
}