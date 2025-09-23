namespace EquityPositions.Api.Repositories.Abstract
{
    public interface IConfigurationRepository
    {
        Task<Dictionary<string, string>> GetConfigurationAsync(string section);
        Task<DateTime> GetLastModifiedAsync(string section);
    }
}
