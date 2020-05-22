using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduling.DataAccess.Extensions;
using Scheduling.Engine.Extensions;
using Scheduling.Engine.Jobs;
using Scheduling.Orchestrator.ServiceBus;

namespace Scheduling.Orchestrator
{
    // NOTE: This webjob could just as well be an IHostedService if that better suites your needs
    public class Startup
    {
        // TODO: Get health checks in place
        // TODO: Get Application Insights in place
        // TODO: Get Loggly in place
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddSchedulingDataAccess(hostContext.Configuration.GetConnectionStringOrSetting("SchedulingConnString"));
                    services.AddSingleton<IServiceBus, ServiceBus.ServiceBus>();

                    services.AddTransient<IScheduledJobExecutor, ScheduledJobExecutor>(); // Important that this is transient scoped
                    services.AddSchedulingEngine(); // Needs to be registered after IScheduledJobExecutor is already in IoC container
                })
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddServiceBus();
                })
                .ConfigureLogging((context, b) => { b.AddConsole(); })
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