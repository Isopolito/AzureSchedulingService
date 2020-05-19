using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.AzureFunctions
{
    public class AddOrUpdateJobFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public AddOrUpdateJobFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        // TODO: Handle dead letters
        public async Task AddOrUpdateJob([ServiceBusTrigger("scheduling-addorupdate")]
                                         Message message, ILogger logger, CancellationToken ct)
        {
            string body = null;
            try
            {
                body = Encoding.UTF8.GetString(message.Body);
                var job = JsonConvert.DeserializeObject<Job>(body);
                await schedulingActions.AddOrUpdateJob(job, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {body}");
            }
        }
    }
}