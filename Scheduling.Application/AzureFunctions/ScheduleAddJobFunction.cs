using System;
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
    public class ScheduleAddJobFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public ScheduleAddJobFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        // TODO: Handle dead letters
        public async Task AddJob([ServiceBusTrigger("scheduling-add")] Message message, ILogger logger, CancellationToken ct)
        {
            string body = null;
            try
            {
                body = Encoding.UTF8.GetString(message.Body);
                var job = JsonConvert.DeserializeObject<ScheduleJobMessage>(body);
                await schedulingActions.AddOrUpdateJob(job, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {body}");
            }
        }
    }
}
