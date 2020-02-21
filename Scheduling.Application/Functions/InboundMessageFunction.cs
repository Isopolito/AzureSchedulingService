using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.Application.ServiceBus;
using Scheduling.SharedPackage;

namespace Scheduling.Application.Functions
{
    public class InboundMessageFunction
    {
        private readonly IServiceBus serviceBus;

        public InboundMessageFunction(IServiceBus serviceBus)
        {
            this.serviceBus = serviceBus;
        }

        // TODO: Handle dead letters
        public async Task ScheduleInboundJobs([ServiceBusTrigger("scheduling-add")] Message message, ILogger<InboundMessageFunction> logger)
        {
            var job = JsonConvert.DeserializeObject<ScheduleJobMessage>(Encoding.UTF8.GetString(message.Body));
            await serviceBus.EnsureSubscriptionIsSetup(job.SubscriptionId);

            var executeJobMessage = new ExecuteJobMessage
            {
                JobUid = job.JobUid,
            };
            await serviceBus.PublishEventToTopic(job.SubscriptionId, JsonConvert.SerializeObject(executeJobMessage));
        }
    }
}
