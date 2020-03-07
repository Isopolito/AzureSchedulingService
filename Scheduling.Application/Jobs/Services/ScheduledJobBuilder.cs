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
        {
            var inputErrorMessage = ValidateJobBuilderInput(scheduleJobMessage);
            if (!string.IsNullOrEmpty(inputErrorMessage)) return Result.Failure<IJobDetail>(inputErrorMessage);

            return Result.Success(
                JobBuilder.Create<ScheduledJob>()
                    .WithIdentity(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionName)
                    .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid)
                    .UsingJobData(SchedulingConstants.SubscriptionName, scheduleJobMessage.SubscriptionName)
                    .Build());
        }

        public Result<ITrigger> BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
        {
            var inputErrorMessage = ValidateTriggerBuilderInput(schedule);
            if (!string.IsNullOrEmpty(inputErrorMessage)) return Result.Failure<ITrigger>(inputErrorMessage);

            if (!string.IsNullOrEmpty(schedule.CronOverride))
            {
                return Result.Success(
                    TriggerBuilder.Create()
                        .WithIdentity(jobUid, subscriptionName)
                        .WithCronSchedule(schedule.CronOverride)
                        .Build());
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

            return Result.Success(trigger.Build());
        }

        private static string ValidateJobBuilderInput(ScheduleJobMessage scheduleJobMessage)
        {
            if (string.IsNullOrEmpty(scheduleJobMessage.JobUid) || string.IsNullOrEmpty(scheduleJobMessage.SubscriptionName))
            {
                return "JobUid and SubscriptionName are required for scheduling a job";
            }

            if (scheduleJobMessage.Schedule == null)
            {
                return "Schedule property is required in order to schedule job";
            }

            return null;
        }

        private static string ValidateTriggerBuilderInput(JobSchedule schedule)
        {
            if (schedule == null) return "Schedule property is required in order to schedule job";

            if (schedule.RepeatInterval.HasValue
                && schedule.RepeatInterval.Value < TimeSpan.FromMilliseconds(SchedulingConstants.MinimumRepeatIntervalInMs))
            {
                return $"Scheduling RepeatInterval time must be a greater then or equal to {SchedulingConstants.MinimumRepeatIntervalInMs}ms";
            }

            if (schedule.RepeatCount > 0 && !schedule.RepeatInterval.HasValue)
            {
                return "A RepeatCount must also have a RepeatInterval provided";
            }

            if (string.IsNullOrEmpty(schedule.CronOverride) && schedule.StartAt < DateTime.Now && schedule.RepeatCount < 1)
            {
                return "StartAt cannot be a date in the past if the job is not set to repeat";
            }

            if (schedule.EndAt.HasValue && schedule.EndAt < DateTime.Now)
            {
                return "EndAt cannot be a date in the past";
            }

            if (schedule.RepeatCount < 0)
            {
                return "Scheduled RepeatCount cannot be a negative number";
            }

            return null;
        }
    }
}