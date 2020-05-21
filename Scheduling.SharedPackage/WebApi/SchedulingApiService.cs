using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scheduling.SharedPackage.Models;

namespace Scheduling.SharedPackage.WebApi
{
    public class SchedulingApiService : BaseApiService, ISchedulingApiService
    {
        private readonly FunctionKeys functionKeys;

        public SchedulingApiService(FunctionKeys functionKeys, IHttpClientFactory httpClientFactory, string baseAddress) 
            : base(httpClientFactory, baseAddress)
        {
            this.functionKeys = functionKeys;
        }

        public async Task AddOrUpdateJob(Job job)
        {
            const string path = "/api/Job";

            var json = JsonConvert.SerializeObject(job);
            await PutAsync(json, path, functionKeys.AddOrUpdateJob);
        }

        public async Task<Job> GetJob(JobLocator jobLocator)
        {
            return await GetAsync<Job>(JobLocatorToPath(jobLocator), functionKeys.GetJob);
        }

        public async Task DeleteJob(JobLocator jobLocator)
        {
            await DeleteAsync(JobLocatorToPath(jobLocator), functionKeys.DeleteJob);
        }

        private static string JobLocatorToPath(JobLocator jobLocator) => $"/api/Job/{jobLocator.SubscriptionName}/{jobLocator.JobIdentifier}";
    }
}