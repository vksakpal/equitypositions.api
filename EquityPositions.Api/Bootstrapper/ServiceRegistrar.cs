using EquityPositions.Api.Repositories;
using EquityPositions.Api.Repositories.Abstract;
using EquityPositions.Api.Services.Abstract;
using EquityPositions.Api.Services.Concrete;

namespace EquityPositions.Api.Bootstrapper
{
    public static class ServiceRegistrar
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddControllers();
            
            // Add Swagger services
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Equity Positions API",
                    Version = "v1",
                    Description = "API for managing equity positions and transactions"
                });
            });

            //App Specific Services
            services.AddScoped<IPositionCalculator, PositionCalculator>();
            services.AddScoped<IRepository, TransactionRepository>();
            services.AddScoped<IEquityPositionService, EquityPositionService>();
            return services;

        }
    }
}
