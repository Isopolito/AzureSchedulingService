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
                subscriptionId = dataMap.GetString(SchedulingConstants.SubscriptionId);
                jobUid = dataMap.GetString(SchedulingConstants.JobUid);

                var executeJobMessage = new ExecuteJobMessage
                {
                    JobUid = Guid.Parse(jobUid),
                };

                await serviceBus.EnsureSubscriptionIsSetup(subscriptionId);
                await serviceBus.PublishEventToTopic(subscriptionId, JsonConvert.SerializeObject(executeJobMessage));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to execute job--jobUid: {jobUid}, subscriptionId: {subscriptionId}");
            }
        }
    }
}
