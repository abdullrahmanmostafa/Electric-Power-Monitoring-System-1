namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalHubs { get; set; }
        public int TotalPlugs { get; set; }
        public int TotalReadings { get; set; }
        public int TotalNotifications { get; set; }
        public Dictionary<string, int> HubUserCounts { get; set; } = new();
    }
}