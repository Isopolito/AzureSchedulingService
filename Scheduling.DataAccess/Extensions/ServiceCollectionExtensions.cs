using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.DataAccess.Contexts;

namespace Scheduling.DataAccess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSchedulingDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<SchedulingContext>(options => { options.UseSqlServer(connectionString); });
        }

    }
}