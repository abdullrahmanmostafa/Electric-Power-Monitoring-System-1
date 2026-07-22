namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminMeterReadingDto
    {
        public long Id { get; set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public decimal ReadingValueWh { get; set; }
        public decimal? BalanceEgp { get; set; }
        public DateTime ReadingDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}