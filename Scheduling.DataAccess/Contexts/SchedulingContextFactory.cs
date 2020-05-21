using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Scheduling.DataAccess.Contexts
{
    internal class SchedulingContextFactory : IDesignTimeDbContextFactory<SchedulingContext>
    {
        private const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
        private const string ConnectionStringName = "SchedulingConnString";

        public SchedulingContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var environmentName = Environment.GetEnvironmentVariable(AspNetCoreEnvironment);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Local.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            var optionsBuilder = new DbContextOptionsBuilder<SchedulingContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new SchedulingContext(optionsBuilder.Options);
        }
    }
}