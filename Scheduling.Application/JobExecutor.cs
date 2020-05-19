using System.Threading.Tasks;
using Newtonsoft.Json;
using Scheduling.Application.ServiceBus;
using Scheduling.Engine.Jobs;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application
{
    public class JobExecutor : IJobExecutor
    {
        private readonly IServiceBus serviceBus;

        public JobExecutor(IServiceBus serviceBus)
        {
            this.serviceBus = serviceBus;
        }

        public async Task Execute(string subscriptionName, string jobIdentifier)
        {
            await serviceBus.EnsureSubscriptionIsSetup(subscriptionName);

            // TODO: Pull job metadata from db (i.e. scheduling.Job) and publish that
            var job = new Job("pulse", "test 1234", "ahall");

            await serviceBus.PublishEventToTopic(subscriptionName, JsonConvert.SerializeObject(job));
        }
    }
}