namespace EquityPositions.Api.Models
{
    public class ConfigurationSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
