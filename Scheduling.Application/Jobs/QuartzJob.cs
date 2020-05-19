using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.Application.ServiceBus;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.Jobs
{
    public class QuartzJob : IJob
    {
        private readonly IServiceBus serviceBus;
        private readonly ILogger<QuartzJob> logger;

        public QuartzJob(IServiceBus serviceBus, ILogger<QuartzJob> logger)
        {
            this.serviceBus = serviceBus;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string subscriptionName = string.Empty, jobIdentifier = string.Empty;
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                subscriptionName = dataMap.GetString(SchedulingConstants.SubscriptionName);
                jobIdentifier = dataMap.GetString(SchedulingConstants.JobIdentifier);

                // TODO: Look up job in scheduling.Job table based on subsciptionName, jobIdentifier

                var job = new Job(subscriptionName, jobIdentifier, "fixme");

                await serviceBus.EnsureSubscriptionIsSetup(subscriptionName);
                await serviceBus.PublishEventToTopic(subscriptionName, JsonConvert.SerializeObject(job));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to execute job--jobIdentifier: {jobIdentifier}, subscriptionName: {subscriptionName}");
            }
        }
    }
}
