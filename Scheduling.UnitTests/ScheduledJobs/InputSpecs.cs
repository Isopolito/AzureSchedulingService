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
                JobUid = Guid.NewGuid(),
                SubscriptionId = "foo",
                Schedule = null,
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Throw_Exception_If_SubscriptionId_Is_Null()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionId = null,
                JobUid = Guid.NewGuid(),
                Schedule = new JobSchedule(),
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void Throw_Exception_If_SubscriptionId_Is_Empty()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionId = "",
                JobUid = Guid.NewGuid(),
                Schedule = new JobSchedule(),
            };

            scheduledJobBuilder.Invoking(y => y.AssertInputIsValid(message))
                .Should().Throw<ArgumentException>()
                .Where(m => m.Message.Contains("SubscriptionId"));
        }

        [Test]
        public void Throw_Exception_If_JobUid_Is_Missing()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionId = "foo",
                JobUid = Guid.Empty,
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
                SubscriptionId = "foo",
                JobUid = Guid.NewGuid(),
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
                SubscriptionId = "foo",
                JobUid = Guid.NewGuid(),
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
                SubscriptionId = "foo",
                JobUid = Guid.NewGuid(),
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
                SubscriptionId = "foo",
                JobUid = Guid.NewGuid(),
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