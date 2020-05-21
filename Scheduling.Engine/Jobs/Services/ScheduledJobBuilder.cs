using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Quartz;
using Quartz.Spi;
using Scheduling.Engine.Constants;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Extensions;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Jobs.Services
{
    public class ScheduledJobBuilder : IScheduledJobBuilder
    {
        private readonly ICronExpressionGenerator cronExpressionGenerator;

        public ScheduledJobBuilder(ICronExpressionGenerator cronExpressionGenerator)
        {
            this.cronExpressionGenerator = cronExpressionGenerator;
        }

        public Result<IJobDetail> BuildJob(Job job)
            => ValidateJobBuilderInput(job)
                .OnFailureCompensate(error => Result.Failure($"Unable to build job: {error}"))
                .OnSuccessTry(() => JobBuilder.Create<QuartzJob>()
                    .WithIdentity(job.JobIdentifier, job.SubscriptionName)
                    .UsingJobData(SchedulingConstants.JobIdentifier, job.JobIdentifier)
                    .UsingJobData(SchedulingConstants.SubscriptionName, job.SubscriptionName)
                    .Build());

        public Result<IReadOnlyList<ITrigger>> BuildTriggers(Job job) => ValidateTriggerBuilderInput(job)
                .OnFailureCompensate(error => Result.Failure($"Unable to build trigger: {error}"))
                .OnSuccessTry(() => BuildScheduleTriggers(job));

        private IReadOnlyList<ITrigger> BuildScheduleTriggers(Job job) 
            => BuildBaseTriggers(job)
                .Select(trigger =>
                {
                    if (job.RepeatEndStrategy == RepeatEndStrategy.OnEndDate && job.EndAt.HasValue)
                    {
                        trigger.EndAt(job.EndAt);
                    }
                    else if (job.RepeatEndStrategy == RepeatEndStrategy.AfterOccurrenceNumber && job.RepeatOccurrenceNumber > 0)
                    {
                        // the trigger must first be built in order to calculate end date from repeat count 
                        var builtTrigger = trigger.Build();
                        var endDate = TriggerUtils.ComputeEndTimeToAllowParticularNumberOfFirings(builtTrigger as IOperableTrigger, null, job.RepeatOccurrenceNumber);
                        trigger.EndAt(endDate);
                    }

                    return trigger.Build();
                })
                .ToList()
                .AsReadOnly();

        private IReadOnlyList<TriggerBuilder> BuildBaseTriggers(Job job)
        {
            if ((job.RepeatInterval == RepeatInterval.Never || job.RepeatInterval == RepeatInterval.NotUsed)
                && job.CronExpressionOverride.HasNoValue())
            {
                // Run once
                return new[]
                {
                    TriggerBuilder.Create()
                        .WithIdentity(job.JobIdentifier, job.SubscriptionName)
                        .WithSimpleSchedule()
                        .StartAt(job.StartAt)
                };
            }

            // Certain scheduling requirements may require multiple cron expressions, which means multiple triggers for a job
            var cronExpressions = cronExpressionGenerator.Create(job);
            return cronExpressions.Select((cronExpression, idx) => TriggerBuilder.Create()
                    .WithIdentity($"{job.JobIdentifier}-{idx}", job.JobIdentifier)
                    .WithCronSchedule(cronExpression)
                    .StartAt(job.StartAt))
                .ToList()
                .AsReadOnly();
        }

        private static Result ValidateJobBuilderInput(Job job)
        {
            if (job.JobIdentifier.HasNoValue() || job.SubscriptionName.HasNoValue())
            {
                return Result.Failure("JobIdentifier and SubscriptionName are required for scheduling a job");
            }

            if (job.CronExpressionOverride.HasNoValue() && job.StartAt == DateTime.MinValue)
            {
                return Result.Failure("A schedule must be provided for a job");
            }

            return Result.Success();
        }

        private static Result ValidateTriggerBuilderInput(Job job)
        {
            if (job == null) return Result.Failure("Schedule property is required in order to schedule job");

            if (job.EndAt.HasValue && job.EndAt.Value.ToUniversalTime() < DateTime.UtcNow)
            {
                return Result.Failure("EndAt cannot be a date in the past");
            }

            if (job.RepeatOccurrenceNumber < 0)
            {
                return Result.Failure("Scheduled RepeatCount cannot be a negative number");
            }

            if (job.RepeatEndStrategy == RepeatEndStrategy.AfterOccurrenceNumber
                && job.RepeatOccurrenceNumber < 1 && job.StartAt.ToUniversalTime() < DateTime.UtcNow)
            {
                return Result.Failure("StartAt cannot be in the past for non-repeating jobs");
            }

            if (job.RepeatEndStrategy == RepeatEndStrategy.OnEndDate && job.EndAt < job.StartAt)
            {
                return Result.Failure("EndAt cannot be before StartDate");
            }

            if ((job.RepeatInterval == RepeatInterval.Never || job.RepeatInterval == RepeatInterval.NotUsed)
                && job.StartAt.ToUniversalTime() < DateTime.UtcNow && job.CronExpressionOverride.HasNoValue())
            {
                return Result.Failure("StartAt cannot be in the past for non-repeating jobs");
            }

            return Result.Success();
        }
    }
}