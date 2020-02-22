using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.Application.Scheduling;
using Scheduling.SharedPackage.Messages;

namespace Scheduling.Application.Functions
{
    public class InboundMessageFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public InboundMessageFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        // TODO: Handle dead letters
        public async Task ScheduleInboundJobs([ServiceBusTrigger("scheduling-add")] Message message, ILogger logger, CancellationToken ct)
        {
            var job = JsonConvert.DeserializeObject<ScheduleJobMessage>(Encoding.UTF8.GetString(message.Body));
            await schedulingActions.AddOrUpdateJob(job, ct);
        }
    }
}
