namespace EquityPositions.Api.Models
{
    // Configuration model to represent your settings
    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableRetry { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
        public string Environment { get; set; } = "Development";
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }
}
