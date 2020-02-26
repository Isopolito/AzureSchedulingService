using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Jobs;
using Scheduling.Application.Services.Jobs;
using Scheduling.Application.Services.Scheduling;
using Scheduling.Application.Services.ServiceBus;

namespace Scheduling.Application
{
    public class Startup
    {
        // TODO: Get health checks in place
        // TODO: Get Application Insights in place
        // TODO: Get Loggly in place
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();

                    services.AddSingleton<IServiceBus, ServiceBus>();
                    services.AddSingleton<ISchedulingActions, SchedulingActions>();
                    services.AddSingleton<IScheduledJobBuilder, ScheduledJobBuilder>();

                    services.AddTransient<ScheduledJob>();
                })
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddServiceBus();
                })
                .ConfigureLogging((context, b) =>
                {
                    b.AddConsole();
                })
                .Build();

            using (host)
            {
                // Start scheduler: it will continuously monitor and look for jobs that need to be executed
                // Need to set up a customized IJobFactory in order to use the service container so that the IJob implementations can leverage DI
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                var inputs = new Dictionary<string, object>
                {
                    { "jobFactory", new JobFactory(host.Services) }
                };
                await jobHost.CallAsync("InitiateScheduler", inputs);

                // Run other functions that listen for inbound messages
                await host.RunAsync();
            }
        }
    }
}
