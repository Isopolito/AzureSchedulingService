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
using Scheduling.SharedPackage.Extensions;

namespace Scheduling.Orchestrator
{
    // NOTE: This webjob could just as well be an IHostedService if that better suites your needs
    public class Startup
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var conf = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                    config.AddConfiguration(conf);
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddServiceBus();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddSingleton<IServiceBus, ServiceBus.ServiceBus>();
                    services.AddSchedulingDataAccess(hostContext.Configuration.GetConnectionStringOrSetting("SchedulingConnString"));
                    services.AddTransient<IScheduledJobExecutor, ScheduledJobExecutor>(); // Important that this is transient scoped
                    services.AddSchedulingEngine(); // Needs to be registered after IScheduledJobExecutor is already in IoC container
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Information);
                    b.AddConsole();

                    var appInsightsKey = context.Configuration["ApplicationInsights:InstrumentationKey"];
                    if (appInsightsKey.HasValue()) b.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);
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