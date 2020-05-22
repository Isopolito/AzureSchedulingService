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

        public async Task DeleteJob([ServiceBusTrigger(MessageQueueNames.Delete)]
                                    Message message, ILogger logger, CancellationToken ct)
        {
            try
            {
                await Delete(message, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to delete job. Message: {message.Body}");
            }
        }

        public async Task DeleteFunctionDeadLetter([ServiceBusTrigger(MessageQueueNames.Delete + "/$DeadLetterQueue")]
                                                   Message message,
                                                   ILogger logger,
                                                   CancellationToken ct)
        {
            try
            {
                logger.LogInformation($"Processing dead letter in {GetType().Name}. Message body: {Encoding.UTF8.GetString(message.Body)}");
                var spanInMinutes = (DateTime.Now - message.ScheduledEnqueueTimeUtc).TotalMinutes;
                if (spanInMinutes > 15) return; // Ignore dead letter messages that have been sitting for longer than 15 minutes

                await Delete(message, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {message.Body}");
            }
        }

        private async Task Delete(Message message, CancellationToken ct)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            var job = JsonConvert.DeserializeObject<JobLocator>(body);
            await schedulingActions.DeleteJob(job, ct);
        }
    }
}