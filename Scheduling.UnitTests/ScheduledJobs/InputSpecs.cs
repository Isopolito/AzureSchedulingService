using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scheduling.Application.Jobs.Services;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class InputSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;

        [SetUp]
        public void Setup()
        {
            var nullLogger = new Logger<ScheduledJobBuilder>(new NullLoggerFactory());
            scheduledJobBuilder = new ScheduledJobBuilder(nullLogger);
        }

        [Test]
        public void Throw_Exception_If_Schedule_Is_Missing()
        {
            var message = new ScheduleJobMessage
            {
                JobUid = "unique id 1234",
                SubscriptionName = "foo",
                Schedule = null,
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Throw_Exception_If_SubscriptionName_Is_Null()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = null,
                JobUid = "unique id 1234",
                Schedule = new JobSchedule(),
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Throw_Exception_If_SubscriptionName_Is_Empty()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = string.Empty,
                JobUid = "unique id 1234",
                Schedule = new JobSchedule(),
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("SubscriptionName"));
        }

        [Test]
        public void Throw_Exception_If_JobUid_Is_Missing()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule(),
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("JobUid"));
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

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("EndAt"));
        }

        [Test]
        public void Throw_Exception_If_Scheduled_StartAt_Is_In_The_Past()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    StartAt = DateTime.Now,
                }
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("StartAt"));
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

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("RepeatCount"));
        }

        [Test]
        public void Throw_Exception_If_RepeatInterval_Is_Less_Than_The_Minimum_Requirement()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    RepeatInterval = TimeSpan.FromMilliseconds(5),
                    StartAt = DateTime.Now.AddSeconds(5),
                }
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("RepeatInterval"));
        }
    }
}