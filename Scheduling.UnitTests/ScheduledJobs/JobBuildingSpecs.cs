using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scheduling.Application.Services.Jobs;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class JobBuildingSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;
        private ScheduleJobMessage defaultMessage;

        [SetUp]
        public void Setup()
        {
            var nullLogger = new Logger<ScheduledJobBuilder>(new NullLoggerFactory());
            scheduledJobBuilder = new ScheduledJobBuilder(nullLogger);
            defaultMessage = new ScheduleJobMessage
            {
                JobUid = Guid.NewGuid(),
                SubscriptionId = "foo",
            };
        }

        [Test]
        public void StartAt_On_Trigger_Should_Match_Whats_In_The_Message_Schedule()
        {
            var startAt = DateTime.Now;
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = startAt,
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule);

            trigger.StartTimeUtc.Should().Be(startAt.ToUniversalTime());
        }

        [Test]
        public void EndAt_On_Trigger_Should_Match_Whats_In_The_Message_Schedule()
        {
            var endAt = DateTime.Now.AddDays(1);
            defaultMessage.Schedule = new JobSchedule
            {
                EndAt = endAt,
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule);

            trigger.EndTimeUtc.Should().Be(endAt.ToUniversalTime());
        }

        [Test]
        public void No_EndDate_In_Message_Schedule_Means_No_EndTime_In_Trigger()
        {
            var endAt = DateTime.Now;
            defaultMessage.Schedule = new JobSchedule
            {
                EndAt = endAt,
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule);

            trigger.EndTimeUtc.Should().Be(endAt.ToUniversalTime());
        }

        [Test]
        public void Trigger_Identity_Should_Be_SubscriptionId_And_JobId()
        {
            defaultMessage.Schedule = new JobSchedule();

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule) as Quartz.Impl.Triggers.SimpleTriggerImpl;

            trigger.Name.Should().Be(defaultMessage.JobUid.ToString());
            trigger.Group.Should().Be(defaultMessage.SubscriptionId);
        }

        [Test]
        public void Repeat_Count_In_Trigger_Is_Same_As_In_Message()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                RepeatCount = 99,
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule) as Quartz.Impl.Triggers.SimpleTriggerImpl;

            trigger.RepeatCount.Should().Be(defaultMessage.Schedule.RepeatCount);
        }

        [Test]
        public void Interval_Minutes_In_Trigger_Is_Same_As_In_Message()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                RepeatInterval = TimeSpan.FromMinutes(25),
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule) as Quartz.Impl.Triggers.SimpleTriggerImpl;

            trigger.RepeatInterval.Should().Be(TimeSpan.FromMinutes(25));
        }

        [Test]
        public void Cron_Job_String_Is_Used_When_Provided_In_Message()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                RepeatInterval = TimeSpan.FromMinutes(25),
                CronOverride = "0,24 0,33 0 3,18,22 JAN,MAR,NOV ? *",
            };

            var trigger = scheduledJobBuilder.BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionId, defaultMessage.Schedule) as Quartz.Impl.Triggers.CronTriggerImpl;
            trigger.CronExpressionString.Should().Be(defaultMessage.Schedule.CronOverride);
        }
    }
}