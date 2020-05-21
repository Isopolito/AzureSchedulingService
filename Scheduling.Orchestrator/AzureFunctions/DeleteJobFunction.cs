using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Constants;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Orchestrator.AzureFunctions
{
    public class DeleteJobFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public DeleteJobFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        // TODO: Handle dead letters
        public async Task DeleteJob([ServiceBusTrigger(MessageQueueNames.Delete)] Message message, ILogger logger, CancellationToken ct)
        {
            string body = null;
            try
            {
                body = Encoding.UTF8.GetString(message.Body);
                var job = JsonConvert.DeserializeObject<JobLocator>(body);
                await schedulingActions.DeleteJob(job, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to delete job. Message: {body}");
            }
        }
    }
}