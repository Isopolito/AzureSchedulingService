using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Scheduling.SharedPackage.WebApi;

namespace Scheduling.SharedPackage.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSchedulingApi(this IServiceCollection services, SchedulingApiServiceOptions options)
        {
            services.AddSchedulingApi<ISchedulingApiService, SchedulingApiService>(options);

            services.AddHttpClient("ATISchedulingFunction")
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2));
        }

        private static void AddSchedulingApi<TInterface, TApiService>(this IServiceCollection services, SchedulingApiServiceOptions options) where TApiService : BaseApiService 
            where TInterface : class, IApiService
        {
            services.AddScoped(sp =>
            {
                var serviceAddress = options.ServiceAddressFetcher();
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return Activator.CreateInstance(typeof(TApiService), options.FunctionKeys, factory, serviceAddress) as TInterface;
            });
        }
    }
}
