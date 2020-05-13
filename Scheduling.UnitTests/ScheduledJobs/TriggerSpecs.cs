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
                RepeatInterval = RepeatIntervals.Monthly,
                RepeatEndStrategy = RepeatEndStrategy.Never,
            };

            var triggers = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;
            triggers[0].StartTimeUtc.Should().BeCloseTo(startAt, 1000);
        }

        [Test]
        public void BiMonthly_Should_Result_In_Two_Triggers()
        {
            var endAt = DateTime.Now.AddDays(1);
            defaultMessage.Schedule = new JobSchedule
            {
                EndAt = endAt,
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatInterval = RepeatIntervals.BiMonthly,
            };

            var triggers = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            triggers.Count.Should().Be(2);
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

            var triggers = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            triggers[0].EndTimeUtc.Should().Be(endAt.ToUniversalTime());
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

            var triggers = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            triggers[0].EndTimeUtc.Should().HaveValue();
        }

        [Test]
        public void No_Repeat_End_Strategy_Means_No_EndDate()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(5),
                RepeatEndStrategy = RepeatEndStrategy.Never,
            };

            var triggers = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value;

            triggers[0].EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void Repeat_Interval_But_No_Repeat_Strategy_Means_Job_Never_Ends()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(30),
                RepeatInterval = RepeatIntervals.Weekly,
            };

            var trigger = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value[0];

            trigger.EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void Repeat_Occurrence_Should_Result_In_Correct_EndDate()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    RepeatCount = 12,
                    RepeatEndStrategy = RepeatEndStrategy.AfterOccurrenceNumber,
                    RepeatInterval = RepeatIntervals.Monthly,
                    StartAt = Convert.ToDateTime("5/5/2020"),
                }
            };

            var triggers = scheduledJobBuilder.BuildTriggers(message.JobUid, message.SubscriptionName, message.Schedule).Value;
            triggers[0].EndTimeUtc.Value.Date.ToShortDateString().Should().Be("4/6/2021");
        }

        [Test]
        public void Trigger_Identity_Should_Be_SubscriptionName_And_JobId()
        {
            defaultMessage.Schedule = new JobSchedule
            {
                StartAt = DateTime.Now.AddMinutes(30)
            };

            var trigger = scheduledJobBuilder
                .BuildTriggers(defaultMessage.JobUid, defaultMessage.SubscriptionName, defaultMessage.Schedule)
                .Value[0] as Quartz.Impl.Triggers.SimpleTriggerImpl;

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

            var result = scheduledJobBuilder.BuildTriggers(message.JobUid, message.SubscriptionName, message.Schedule);
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

            var result = scheduledJobBuilder.BuildTriggers(message.JobUid, message.SubscriptionName, message.Schedule);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("StartAt cannot be in the past");
        }

        [Test]
        public void Throw_Exception_If_Repeat_End_At_But_EndDate_Is_Before_StartDate()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    RepeatEndStrategy = RepeatEndStrategy.AfterEndDate,
                    StartAt = DateTime.Now.AddMinutes(5),
                    EndAt = DateTime.Now.AddMinutes(3),
                }
            };

            var result = scheduledJobBuilder.BuildTriggers(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("EndAt cannot be before");
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

            var result = scheduledJobBuilder.BuildTriggers(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("Scheduled RepeatCount cannot be a negative number");
        }
    }
}