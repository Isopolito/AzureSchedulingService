using System;
using CSharpFunctionalExtensions;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Spi;
using Scheduling.Application.Constants;
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

        public Result<ITrigger> BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
            => ValidateTriggerBuilderInput(schedule)
                .OnFailureCompensate(error => Result.Failure($"Unable to build trigger: {error}"))
                .OnSuccessTry(() => BuildScheduleTrigger(jobUid, subscriptionName, schedule));

        private ITrigger BuildScheduleTrigger(string jobUid, string subscriptionName, JobSchedule schedule)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobUid, subscriptionName)
                .WithCronSchedule(cronExpressionGenerator.Create(schedule))
                .StartAt(schedule.StartAt);

            if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterEndDate && schedule.EndAt.HasValue)
            {
                trigger.EndAt(schedule.EndAt);
            }
            else if (schedule.RepeatEndStrategy == RepeatEndStrategy.AfterOccurrenceNumber && schedule.RepeatCount > 0)
            {
                var endDate = TriggerUtils.ComputeEndTimeToAllowParticularNumberOfFirings(trigger as IOperableTrigger,
                  new BaseCalendar(), schedule.RepeatCount);

                trigger.EndAt(endDate);
            }

            return trigger.Build();
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