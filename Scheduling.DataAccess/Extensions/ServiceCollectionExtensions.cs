using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.DataAccess.Contexts;
using Scheduling.DataAccess.Repositories;

namespace Scheduling.DataAccess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSchedulingDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SchedulingContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddTransient<IJobRepository, JobRepository>();

            return services;
        }
    }
}