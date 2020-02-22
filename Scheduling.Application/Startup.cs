using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using Scheduling.Application.Logging;
using Scheduling.Application.Scheduling;
using Scheduling.Application.ServiceBus;

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

                    services.AddSingleton<IServiceBus, ServiceBus.ServiceBus>();
                    services.AddSingleton<ISchedulingActions, SchedulingActions>();
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
                // Need to set up a customized IJobFactory to use the service container so that IJob implementations can use DI
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
