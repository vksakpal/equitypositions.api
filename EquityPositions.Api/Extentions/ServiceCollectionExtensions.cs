using EquityPositions.Api.OptionsMonitor;
using EquityPositions.Api.Repositories;
using EquityPositions.Api.Repositories.Abstract;
using Microsoft.Extensions.Options;

namespace EquityPositions.Api.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseOptionsMonitor<TOptions>(
            this IServiceCollection services,
            string connectionString,
            string sectionName = "")
            where TOptions : class, new()
        {
            // Register dependencies
            services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
            services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();

            // Register the custom options monitor
            services.AddSingleton<IOptionsMonitor<TOptions>>(provider =>
            {
                var repository = provider.GetRequiredService<IConfigurationRepository>();
                var logger = provider.GetRequiredService<ILogger<DatabaseOptionsMonitor<TOptions>>>();
                return new DatabaseOptionsMonitor<TOptions>(repository, logger, sectionName);
            });

            return services;
        }
    }
}
