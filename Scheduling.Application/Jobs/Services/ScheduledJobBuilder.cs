using System;
using CSharpFunctionalExtensions;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public class ScheduledJobBuilder : IScheduledJobBuilder
    {
        public Result<IJobDetail> BuildJob(ScheduleJobMessage scheduleJobMessage)
            => ValidateJobBuilderInput(scheduleJobMessage)
                .OnFailureCompensate(error => Result.Failure($"Unable to build job: {error}"))
                .OnSuccessTry(() => JobBuilder.Create<ScheduledJob>()
                        .WithIdentity(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionName)
                        .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid)
                        .UsingJobData(SchedulingConstants.SubscriptionName, scheduleJobMessage.SubscriptionName)
                        .Build());

        public Result<ITrigger> BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
            => ValidateTriggerBuilderInput(schedule)
                .OnFailureCompensate(error => Result.Failure($"Unable to build trigger: {error}"))
                .OnSuccessTry(() => string.IsNullOrEmpty(schedule.CronOverride) 
                    ? BuildScheduleConfiguredTrigger(jobUid, subscriptionName, schedule) 
                    : BuildCronConfiguredTrigger(jobUid, subscriptionName, schedule));

        private static ITrigger BuildScheduleConfiguredTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
        {
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

        private static ITrigger BuildCronConfiguredTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
            => TriggerBuilder.Create()
                .WithIdentity(jobUid, subscriptionName)
                .WithCronSchedule(schedule.CronOverride)
                .Build();

        private static Result ValidateJobBuilderInput(ScheduleJobMessage scheduleJobMessage)
        {
            if (string.IsNullOrEmpty(scheduleJobMessage.JobUid) || string.IsNullOrEmpty(scheduleJobMessage.SubscriptionName))
            {
                return Result.Failure("JobUid and SubscriptionName are required for scheduling a job");
            }

            if (scheduleJobMessage.Schedule == null)
            {
                return Result.Failure("Schedule property is required in order to schedule job");
            }

            return Result.Success();
        }

        private static Result ValidateTriggerBuilderInput(JobSchedule schedule)
        {
            if (schedule == null) return Result.Failure("Schedule property is required in order to schedule job");

            if (schedule.RepeatInterval.HasValue
                && schedule.RepeatInterval.Value < TimeSpan.FromMilliseconds(SchedulingConstants.MinimumRepeatIntervalInMs))
            {
                return Result.Failure($"Scheduling RepeatInterval time must be a greater then or equal to {SchedulingConstants.MinimumRepeatIntervalInMs}ms");
            }

            if (schedule.RepeatCount > 0 && !schedule.RepeatInterval.HasValue)
            {
                return Result.Failure("A RepeatCount must also have a RepeatInterval provided");
            }

            if (string.IsNullOrEmpty(schedule.CronOverride) && schedule.StartAt < DateTime.Now && schedule.RepeatCount < 1)
            {
                return Result.Failure("StartAt cannot be a date in the past if the job is not set to repeat");
            }

            if (schedule.EndAt.HasValue && schedule.EndAt < DateTime.Now)
            {
                return Result.Failure("EndAt cannot be a date in the past");
            }

            if (schedule.RepeatCount < 0)
            {
                return Result.Failure("Scheduled RepeatCount cannot be a negative number");
            }

            return Result.Success();
        }
    }
}