using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.Application.ServiceBus;
using Scheduling.SharedPackage.Messages;

namespace Scheduling.Application.Scheduling
{
    public class ScheduledJob : IJob
    {
        private readonly IServiceBus serviceBus;
        private readonly ILogger<ScheduledJob> logger;

        public ScheduledJob(IServiceBus serviceBus, ILogger<ScheduledJob> logger)
        {
            this.serviceBus = serviceBus;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string subscriptionId = "", jobUid = "";
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                subscriptionId = dataMap.GetString(JobConstants.SubscriptionId);
                jobUid = dataMap.GetString(JobConstants.JobUid);

                await serviceBus.EnsureSubscriptionIsSetup(subscriptionId);
                var executeJobMessage = new ExecuteJobMessage
                {
                    JobUid = Guid.Parse(jobUid),
                };
                await serviceBus.PublishEventToTopic(subscriptionId, JsonConvert.SerializeObject(executeJobMessage));

                //var key = context.JobDetail.Key;
                //await Console.Error.WriteLineAsync("Instance " + key + " of DumbJob says: " + jobSays + ", and val is: " + myFloatValue);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to execute job--jobUid: {jobUid}, subscriptionId: {subscriptionId}");
                throw;
            }

        }
    }
}
