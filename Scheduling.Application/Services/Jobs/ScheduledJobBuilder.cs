using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.Application.Jobs;
using Scheduling.SharedPackage.Enumerations;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Services.Jobs
{
    public class ScheduledJobBuilder : IScheduledJobBuilder
    {
        private readonly ILogger<ScheduledJobBuilder> logger;

        public ScheduledJobBuilder(ILogger<ScheduledJobBuilder> logger)
        {
            this.logger = logger;
        }

        public  IJobDetail BuildJob(ScheduleJobMessage scheduleJobMessage)
            => JobBuilder.Create<ScheduledJob>()
                .WithIdentity(scheduleJobMessage.JobUid.ToString(), scheduleJobMessage.SubscriptionId)
                .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid.ToString())
                .UsingJobData(SchedulingConstants.SubscriptionId, scheduleJobMessage.SubscriptionId)
                .Build();

        public void AssertInputIsValid(ScheduleJobMessage scheduleJobMessage)
        {
            if (scheduleJobMessage.Schedule?.ExecutionInterval == null)
            {
                throw new ArgumentException($"Schedule configuration, including ExecutionInterval, is required. Message: {JsonConvert.SerializeObject(scheduleJobMessage)}");
            }

            if (scheduleJobMessage.JobUid == Guid.Empty || string.IsNullOrEmpty(scheduleJobMessage.SubscriptionId))
            {
                throw new ArgumentException($"JobUid and SubscriptionId are required for scheduling a job. Message: {JsonConvert.SerializeObject(scheduleJobMessage)}");
            }
        }

        public ITrigger BuildTrigger(Guid jobUid, string subscriptionId, JobSchedule schedule)
        {
            try
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(jobUid.ToString(), subscriptionId)
                    .StartAt(schedule.StartAt);

                if (schedule.EndAt.HasValue)
                {
                    trigger.EndAt(schedule.EndAt);
                }

                var simpleSchedule = SimpleScheduleBuilder.Create();
                if (schedule.RepeatCount > 0)
                {
                    simpleSchedule.WithRepeatCount(schedule.RepeatCount);
                }

                var intervalMultiplierInMinutes = 1;
                if (schedule.ExecutionInterval.IntervalPeriod == IntervalPeriods.Hours)
                {
                    intervalMultiplierInMinutes = 60;
                }
                else if (schedule.ExecutionInterval.IntervalPeriod == IntervalPeriods.Days)
                {
                    intervalMultiplierInMinutes = 60 * 24;
                }

                simpleSchedule.WithIntervalInMinutes(schedule.ExecutionInterval.IntervalValue * intervalMultiplierInMinutes);
                trigger.WithSchedule(simpleSchedule);

                return trigger.Build();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to create trigger for jobUid {jobUid}, subscriptionId {subscriptionId}. Schedule: {JsonConvert.SerializeObject(schedule)}");
                throw;
            }
        }
    }
}