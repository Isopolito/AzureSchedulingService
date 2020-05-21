﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Scheduling.Engine.Constants;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Jobs
{
    public class QuartzJob : IJob
    {
        private readonly ILogger<QuartzJob> logger;
        private readonly IScheduledJobExecutor executor;

        public QuartzJob(IScheduledJobExecutor executor, ILogger<QuartzJob> logger)
        {
            this.executor = executor;
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
                await executor.Execute(new JobLocator(subscriptionName, jobIdentifier));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to execute job--jobIdentifier: {jobIdentifier}, subscriptionName: {subscriptionName}");
            }
        }
    }
}
