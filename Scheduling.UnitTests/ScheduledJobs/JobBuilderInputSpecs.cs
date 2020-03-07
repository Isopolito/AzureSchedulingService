using FluentAssertions;
using NUnit.Framework;
using Scheduling.Application.Jobs.Services;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class JobBuilderInputSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;

        [SetUp]
        public void Setup()
        {
            scheduledJobBuilder = new ScheduledJobBuilder();
        }

        [Test]
        public void BuildJob_Throw_Exception_If_Schedule_Is_Missing()
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
        public void Throw_Exception_If_SubscriptionName_Is_Null()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = null,
                JobUid = "unique id 1234",
                Schedule = new JobSchedule(),
            };

            var result = scheduledJobBuilder.BuildJob(message);
            result.Error.Should().Contain("JobUid and SubscriptionName are required");
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

            var result = scheduledJobBuilder.BuildJob(message);
            result.Error.Should().Contain("JobUid and SubscriptionName are required");
        }

        [Test]
        public void Throw_Exception_If_JobUid_Is_Null()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = null,
                Schedule = new JobSchedule(),
            };

            var result = scheduledJobBuilder.BuildJob(message);
            result.Error.Should().Contain("JobUid and SubscriptionName are required");
        }

        [Test]
        public void Throw_Exception_If_JobUid_Is_Empty()
        {
            var message = new ScheduleJobMessage
            {
                SubscriptionName = "foo",
                JobUid = string.Empty,
                Schedule = new JobSchedule(),
            };

            var result = scheduledJobBuilder.BuildJob(message);
            result.Error.Should().Contain("JobUid and SubscriptionName are required");
        }
    }
}