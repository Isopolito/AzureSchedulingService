using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Scheduling.Api;
using Scheduling.DataAccess.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Scheduling.Api
{
    // TODO: Get health checks in place
    // TODO: Get Application Insights in place
    // TODO: Get Loggly in place
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connString = config["SchedulingConnString"];
            builder.Services.AddSchedulingDataAccess(connString);
        }
    }
}
