using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.Application.ServiceBus;
using Scheduling.SharedPackage.Messages;

namespace Scheduling.Application.Jobs
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
            string subscriptionName = string.Empty, jobUid = string.Empty;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                subscriptionName = dataMap.GetString(SchedulingConstants.SubscriptionName);
                jobUid = dataMap.GetString(SchedulingConstants.JobUid);

                var executeJobMessage = new ExecuteJobMessage
                {
                    JobUid = jobUid,
                };

                await serviceBus.EnsureSubscriptionIsSetup(subscriptionName);
                await serviceBus.PublishEventToTopic(subscriptionName, JsonConvert.SerializeObject(executeJobMessage));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to execute job--jobUid: {jobUid}, subscriptionName: {subscriptionName}");
            }
        }
    }
}
