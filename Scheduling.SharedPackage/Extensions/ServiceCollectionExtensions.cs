using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Scheduling.SharedPackage.WebApi;

namespace Scheduling.SharedPackage.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string HttpClientName = "ATISchedulingFunction";

        public static void AddSchedulingApi(this IServiceCollection services, SchedulingApiServiceOptions options)
        {
            services.AddScoped(sp =>
            {
                var serviceAddress = options.ServiceAddressFetcher();
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return Activator.CreateInstance(typeof(SchedulingApiService), options.FunctionKeys, factory, serviceAddress) as ISchedulingApiService;
            });

            services.AddHttpClient(HttpClientName)
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2));
        }
    }
}