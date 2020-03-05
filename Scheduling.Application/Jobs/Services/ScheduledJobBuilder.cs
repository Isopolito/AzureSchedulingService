using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public class ScheduledJobBuilder : IScheduledJobBuilder
    {
        private readonly ILogger<ScheduledJobBuilder> logger;

        public ScheduledJobBuilder(ILogger<ScheduledJobBuilder> logger)
        {
            this.logger = logger;
        }

        public IJobDetail BuildJob(ScheduleJobMessage scheduleJobMessage)
            => JobBuilder.Create<ScheduledJob>()
                .WithIdentity(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionName)
                .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid)
                .UsingJobData(SchedulingConstants.SubscriptionName, scheduleJobMessage.SubscriptionName)
                .Build();

        public void AssertInputIsValid(ScheduleJobMessage scheduleJobMessage)
        {
            if (string.IsNullOrEmpty(scheduleJobMessage.JobUid) || string.IsNullOrEmpty(scheduleJobMessage.SubscriptionName))
            {
                throw new ArgumentException("JobUid and SubscriptionName are required for scheduling a job");
            }

            if (scheduleJobMessage.Schedule == null)
            {
                throw new ArgumentException("Schedule property is required in order to schedule job");
            }

            if (scheduleJobMessage.Schedule.RepeatInterval.HasValue 
                && scheduleJobMessage.Schedule.RepeatInterval.Value < TimeSpan.FromMilliseconds(SchedulingConstants.MinimumRepeatIntervalInMs))
            {
                throw new ArgumentException($"Scheduling RepeatInterval time must be a greater then or equal to {SchedulingConstants.MinimumRepeatIntervalInMs}ms");
            }

            if (scheduleJobMessage.Schedule.RepeatCount > 0 && !scheduleJobMessage.Schedule.RepeatInterval.HasValue)
            {
                throw new ArgumentException("A RepeatCount must also have a RepeatInterval provided");
            }

            if (string.IsNullOrEmpty(scheduleJobMessage.Schedule.CronOverride)
                && scheduleJobMessage.Schedule.StartAt < DateTime.Now && scheduleJobMessage.Schedule.RepeatCount < 1)
            {
                throw new ArgumentException("StartAt cannot be a date in the past if the job is not set to repeat");
            }

            if (scheduleJobMessage.Schedule.EndAt.HasValue && scheduleJobMessage.Schedule.EndAt < DateTime.Now)
            {
                throw new ArgumentException("EndAt cannot be a date in the past");
            }

            if (scheduleJobMessage.Schedule.RepeatCount < 0)
            {
                throw new ArgumentException("Scheduled RepeatCount cannot be a negative number");
            }
        }

        public ITrigger BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
        {
            try
            {
                if (!string.IsNullOrEmpty(schedule.CronOverride))
                {
                    return TriggerBuilder.Create()
                        .WithIdentity(jobUid, subscriptionName)
                        .WithCronSchedule(schedule.CronOverride)
                        .Build();
                }

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(jobUid, subscriptionName)
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

                if (schedule.RepeatInterval.HasValue)
                {
                    simpleSchedule.WithInterval(schedule.RepeatInterval.Value);
                }

                trigger.WithSchedule(simpleSchedule);

                return trigger.Build();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to create trigger for jobUid {jobUid}, subscriptionName {subscriptionName}. Schedule: {JsonConvert.SerializeObject(schedule)}");
                throw;
            }
        }
    }
}