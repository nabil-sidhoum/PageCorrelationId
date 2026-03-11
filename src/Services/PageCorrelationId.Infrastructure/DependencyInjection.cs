using PageCorrelationId.Application.Something.Repositories;
using PageCorrelationId.Application.Stats.Repositories;
using PageCorrelationId.Infrastructure.Something;
using PageCorrelationId.Infrastructure.Stats;
using Microsoft.Extensions.DependencyInjection;

namespace PageCorrelationId.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IStatsRepository, StatsRepository>();
            services.AddScoped<ISomethingRepository, SomethingRepository>();

            return services;
        }
    }
}