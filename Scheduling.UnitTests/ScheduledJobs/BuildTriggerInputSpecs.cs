using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.Application.Jobs.Services;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class BuildTriggerInputSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;

        [SetUp]
        public void Setup()
        {
            scheduledJobBuilder = new ScheduledJobBuilder();
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
        public void Throw_Exception_If_RepeatCount_Is_Provided_With_No_RepeatInterval()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = "unique id 1234",
                Schedule = new JobSchedule
                {
                    StartAt = DateTime.Now,
                    RepeatCount = 99,
                }
            };

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("RepeatCount must also have a RepeatInterval");
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
                }
            };

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("StartAt cannot be a date in the past if the job is not set to repeat");
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

            var result = scheduledJobBuilder.BuildTrigger(message.JobUid, message.SubscriptionName, message.Schedule);
            result.Error.Should().Contain("Scheduling RepeatInterval time must be a greater then or equal to");
        }
    }
}