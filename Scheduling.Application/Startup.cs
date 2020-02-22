using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                await jobHost.CallAsync("InitiateScheduler");

                // Run other functions that listen for inbound messages
                await host.RunAsync();
            }
        }
    }
}
