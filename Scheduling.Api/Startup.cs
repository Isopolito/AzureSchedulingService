using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Scheduling.Api;
using Scheduling.DataAccess.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Scheduling.Api
{
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
            builder.Services
                .AddSchedulingDataAccess(connString);
        }
    }
}
