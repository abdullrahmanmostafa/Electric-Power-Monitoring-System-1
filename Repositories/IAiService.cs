namespace Electric_Power_Monitoring_System.Services
{
    public interface IAiService
    {
        Task<List<string>> GenerateTipsAsync(decimal remainingKWh, decimal nextTierPrice);
    }
}