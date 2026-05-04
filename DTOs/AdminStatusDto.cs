namespace Electric_Power_Monitoring_System.DTOs
{
    public class AdminStatusDto
    {
        public double UptimeSeconds { get; set; }
        public long MemoryUsageMB { get; set; }
        public bool DatabaseConnected { get; set; }
        public string ApiVersion { get; set; } = "1.0";
    }
}