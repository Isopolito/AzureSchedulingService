using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.Application.Jobs.Services;
using Scheduling.Application.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class TriggerSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;
        private ScheduleJobMessage defaultMessage;

        [SetUp]
        public void Setup()
        {
            var cronExpressionGenerator = new CronExpressionGenerator();
            scheduledJobBuilder = new ScheduledJobBuilder(cronExpressionGenerator);
            defaultMessage = new ScheduleJobMessage
            {
                JobUid = "unique id 1234",
                SubscriptionName = "foo",
            };
        }

        [Test]
        public void StartAt_On_Trigger_Should_Match_Whats_In_The_Message_Schedule()
        {
            var startAt = DateTime.Now.AddMinutes(3).ToUniversalTime();
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = startAt,
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;
            trigger.StartTimeUtc.Should().BeCloseTo(startAt, 1000);
        }

        [Test]
        public void EndAt_On_Trigger_Should_Match_Whats_In_The_Message_Schedule()
        {
            var endAt = DateTime.Now.AddDays(1);
            defaultMessage.Schedule = new JobSchedule
            {
                EndAt = endAt,
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatInterval = RepeatIntervals.Quarterly,
                RepeatEndStrategy = RepeatEndStrategy.AfterEndDate,
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            trigger.EndTimeUtc.Should().Be(endAt.ToUniversalTime());
        }

        [Test]
        public void Repeat_Count_In_Message_Sets_End_Date()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatInterval = RepeatIntervals.BiMonthly,
                RepeatEndStrategy = RepeatEndStrategy.AfterOccurrenceNumber,
                RepeatCount = 5,
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            trigger.EndTimeUtc.Should().HaveValue();
        }

        [Test]
        public void No_Repeat_Interval_Means_No_EndDate()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatEndStrategy = RepeatEndStrategy.Never,
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            trigger.EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void No_Repeat_Strategy_Means_No_EndDate()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatInterval = RepeatIntervals.Monthly,
                RepeatEndStrategy = RepeatEndStrategy.Never,
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            trigger.EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void Repeat_Interval_But_No_Repeat_Strategy()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(30)
            };

            //var trigger = scheduledJobBuilder
            //        .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
            //        .Value as Quartz.Impl.Triggers.SimpleTriggerImpl;

            //trigger.Name.Should().Be(defaultMessage.JobUid);
            //trigger.Group.Should().Be(defaultMessage.SubscriptionName);

            throw new Exception("Figure this scenario out");
        }

        [Test]
        public void Trigger_Identity_Should_Be_SubscriptionName_And_JobId()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(30)
            };

            var trigger = scheduledJobBuilder
                .BuildTrigger(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value as Quartz.Impl.Triggers.CronTriggerImpl;

            trigger.Name.Should().Be(defaultMessage.JobUid);
            trigger.Group.Should().Be(defaultMessage.SubscriptionName);
        }

        [Test]
        public void BuildTrigger_Throw_Exception_If_Schedule_Is_Missing()
        {
            var message = new ScheduleJobMessage
            {
                JobUid = "unique id 1234",
                SubscriptionName = "foo",
                Schedule = null,
            };

            var result = scheduledJobBuilder.BuildJob(message);
            result.Error.Should().Contain("Schedule property is required ");
        }

        [Test]
        public void Throw_Exception_If_Scheduled_EndAt_Is_In_The_Past()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    EndAt = DateTime.Now.AddMinutes(-1),
                    StartAt = DateTime.Now.AddMinutes(1),
                }
            };

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("EndAt cannot be a date in the past");
        }

        [Test]
        public void Throw_Exception_If_Scheduled_StartAt_Is_In_The_Past_And_Job_Is_Not_Repeating()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    StartAt = DateTime.Now.AddMinutes(-5),
                    RepeatEndStrategy = RepeatEndStrategy.Never,
                    RepeatInterval = RepeatIntervals.Never,
                }
            };

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("StartAt cannot be in the past");
        }

        [Test]
        public void Throw_Exception_If_RepeatCount_Is_Negative()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    RepeatCount = -1,
                    StartAt = DateTime.Now.AddMinutes(5),
                }
            };

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("Scheduled RepeatCount cannot be a negative number");
        }
    }
}