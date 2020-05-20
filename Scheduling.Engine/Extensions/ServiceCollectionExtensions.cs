using Microsoft.Extensions.DependencyInjection;
using Scheduling.Engine.Jobs;
using Scheduling.Engine.Jobs.Services;
using Scheduling.Engine.Scheduling;

namespace Scheduling.Engine.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSchedulingEngine(this IServiceCollection services)
        {
            services.AddSingleton<ISchedulingActions, SchedulingActions>();
            services.AddSingleton<IScheduledJobBuilder, ScheduledJobBuilder>();
            services.AddSingleton<ICronExpressionGenerator, CronExpressionGenerator>();

            services.AddTransient<QuartzJob>();

            return services;
        }
    }
}