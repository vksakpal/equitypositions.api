using Dapper;
using EquityPositions.Api.Repositories.Abstract;
using Microsoft.AspNetCore.Connections;
using System.Data.SqlClient;

namespace EquityPositions.Api.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ConfigurationRepository> _logger;

        public ConfigurationRepository(
            IDbConnectionFactory connectionFactory,
            ILogger<ConfigurationRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> GetConfigurationAsync(string section)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                SELECT [Key], [Value] 
                FROM ConfigurationSettings 
                WHERE Section = @Section AND IsActive = 1";

                var parameters = new { Section = section };
                var results = await connection.QueryAsync<(string Key, string Value)>(sql, parameters);

                return results.ToDictionary(r => r.Key, r => r.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch configuration for section: {Section}", section);
                return new Dictionary<string, string>();
            }
        }

        public async Task<DateTime> GetLastModifiedAsync(string section)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                SELECT MAX(LastModified) 
                FROM ConfigurationSettings 
                WHERE Section = @Section AND IsActive = 1";

                var parameters = new { Section = section };
                var result = await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, parameters);

                return result ?? DateTime.MinValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last modified date for section: {Section}", section);
                return DateTime.MinValue;
            }
        }
    }
}
