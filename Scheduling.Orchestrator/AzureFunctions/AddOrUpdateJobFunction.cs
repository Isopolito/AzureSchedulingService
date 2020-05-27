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
    public class AddOrUpdateFunction
    {
        private readonly ISchedulingActions schedulingActions;
        private readonly ILogger<AddOrUpdateFunction> logger;

        public AddOrUpdateFunction(ISchedulingActions schedulingActions, ILogger<AddOrUpdateFunction> logger)
        {
            this.schedulingActions = schedulingActions;
            this.logger = logger;
        }

        public async Task AddOrUpdateJob([ServiceBusTrigger(MessageQueueNames.AddOrUpdate)]
                                         Message message,
                                         ILogger logger,
                                         CancellationToken ct)
        {
            try
            {
                await AddOrUpdate(message, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {message.Body}");
            }
        }

        public async Task AddOrUpdateDeadLetter([ServiceBusTrigger(MessageQueueNames.AddOrUpdate + "/$DeadLetterQueue")]
                                                Message message,
                                                ILogger logger,
                                                CancellationToken ct)
        {
            logger.LogInformation($"Processing dead letter in {GetType().Name}. Message body: {Encoding.UTF8.GetString(message.Body)}");

            string body = null;
            try
            {
                var spanInMinutes = (DateTime.Now - message.ScheduledEnqueueTimeUtc).TotalMinutes;
                if (spanInMinutes > 15) return; // Ignore dead letter messages that have been sitting for longer than 15 minutes

                await AddOrUpdate(message, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {body}");
            }
        }

        private async Task AddOrUpdate(Message message, CancellationToken ct)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            var job = JsonConvert.DeserializeObject<Job>(body);
            await schedulingActions.AddOrUpdateJob(job, ct);
        }
    }
}