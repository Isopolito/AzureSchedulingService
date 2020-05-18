using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Quartz;
using Quartz.Spi;
using Scheduling.Application.Constants;
using Scheduling.Application.Extensions;
using Scheduling.Application.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public class ScheduledJobBuilder : IScheduledJobBuilder
    {
        private readonly ICronExpressionGenerator cronExpressionGenerator;

        public ScheduledJobBuilder(ICronExpressionGenerator cronExpressionGenerator)
        {
            this.cronExpressionGenerator = cronExpressionGenerator;
        }

        public Result<IJobDetail> BuildJob(ScheduleJobMessage scheduleJobMessage)
            => ValidateJobBuilderInput(scheduleJobMessage)
                .OnFailureCompensate(error => Result.Failure($"Unable to build job: {error}"))
                .OnSuccessTry(() => JobBuilder.Create<ScheduledJob>()
                    .WithIdentity(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionName)
                    .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid)
                    .UsingJobData(SchedulingConstants.SubscriptionName, scheduleJobMessage.SubscriptionName)
                    .Build());

        public Result<IReadOnlyList<ITrigger>> BuildTriggers(string jobUid, string subscriptionName, JobSchedule schedule)
            => ValidateTriggerBuilderInput(schedule)
                .OnFailureCompensate(error => Result.Failure($"Unable to build trigger: {error}"))
                .OnSuccessTry(() => BuildScheduleTriggers(jobUid, subscriptionName, schedule));

        private IReadOnlyList<ITrigger> BuildScheduleTriggers(string jobUid, string subscriptionName, JobSchedule schedule) 
            => BuildBaseTriggers(jobUid, subscriptionName, schedule)
                .Select(trigger =>
                {
                    if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterEndDate && schedule.EndAt.HasValue)
                    {
                        trigger.EndAt(schedule.EndAt);
                    }
                    else if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterOccurrenceNumber && schedule.RepeatCount > 0)
                    {
                        // the trigger must first be built in order to calculate end date from repeat count 
                        var builtTrigger = trigger.Build();
                        var endDate = TriggerUtils.ComputeEndTimeToAllowParticularNumberOfFirings(builtTrigger as IOperableTrigger, null, schedule.RepeatCount);
                        trigger.EndAt(endDate);
                    }

                    return trigger.Build();
                })
                .ToList()
                .AsReadOnly();

        private IReadOnlyList<TriggerBuilder> BuildBaseTriggers(string jobUid, string subscriptionName, JobSchedule schedule)
        {
            if ((schedule.RepeatInterval == RepeatIntervals.Never || schedule.RepeatInterval == RepeatIntervals.Invalid)
                && schedule.CronExpressionOverride.HasNoValue())
            {
                // Run once
                return new[]
                {
                    TriggerBuilder.Create()
                        .WithIdentity(jobUid, subscriptionName)
                        .WithSimpleSchedule()
                        .StartAt(schedule.StartAt)
                };
            }

            // Certain scheduling requirements may require multiple cron expressions, which means multiple triggers for a job
            var cronExpressions = cronExpressionGenerator.Create(schedule);
            return cronExpressions.Select((cronExpression, idx) => TriggerBuilder.Create()
                    .WithIdentity($"{jobUid}-{idx}", subscriptionName)
                    .WithCronSchedule(cronExpression)
                    .StartAt(schedule.StartAt))
                .ToList()
                .AsReadOnly();
        }

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

            if (schedule.EndAt.HasValue && schedule.EndAt.Value.ToUniversalTime() < DateTime.UtcNow)
            {
                return Result.Failure("EndAt cannot be a date in the past");
            }

            if (schedule.RepeatCount < 0)
            {
                return Result.Failure("Scheduled RepeatCount cannot be a negative number");
            }

            if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterOccurrenceNumber
                && schedule.RepeatCount < 1 && schedule.StartAt.ToUniversalTime() < DateTime.UtcNow)
            {
                return Result.Failure("StartAt cannot be in the past for non-repeating jobs");
            }

            if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterEndDate && schedule.EndAt < schedule.StartAt)
            {
                return Result.Failure("EndAt cannot be before StartDate");
            }

            if ((schedule.RepeatInterval == RepeatIntervals.Never || schedule.RepeatInterval == RepeatIntervals.Invalid)
                && schedule.StartAt.ToUniversalTime() < DateTime.UtcNow && schedule.CronExpressionOverride.HasNoValue())
            {
                return Result.Failure("StartAt cannot be in the past for non-repeating jobs");
            }

            return Result.Success();
        }
    }
}