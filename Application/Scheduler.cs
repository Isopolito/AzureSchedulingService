using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application
{
    public class Scheduler
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddServiceBus();
            });
            builder.ConfigureLogging((context, b) =>
            {
                b.AddConsole();
            });
            var host = builder.Build();
            using (host)
            {
                // Continuously run the logic for handling when jobs should run on a different thread 
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                new Thread(async() => await jobHost.CallAsync("WatchForJobsToExecute")).Start();

                // Run other functions that listen for inbound messages
                await host.RunAsync();
            }
        }
    }
}
