using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
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

            // This is a little weird, but the JobFactory needs access to the service provider to create IJob objects through DI
            var jobFactory = new JobFactory(services.BuildServiceProvider());
            services.AddSingleton<IJobFactory>(jobFactory);

            return services;
        }
    }
}