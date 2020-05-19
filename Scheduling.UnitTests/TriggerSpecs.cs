using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.Engine.Jobs.Services;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Models;

namespace Scheduling.UnitTests
{
    public class TriggerSpecs
    {
        private IScheduledJobBuilder scheduledJobBuilder;
        private static Job DefaultJob => new Job("subscription name", "unique identifier", "Tester");

        [SetUp]
        public void Setup()
        {
            var cronExpressionGenerator = new CronExpressionGenerator();
            scheduledJobBuilder = new ScheduledJobBuilder(cronExpressionGenerator);
        }

        [Test]
        public void StartAt_On_Trigger_Should_Match_Whats_In_The_Message_Schedule()
        {
            var job = DefaultJob;
            var startAt = DateTime.Now.AddMinutes(3).ToUniversalTime();
            job.Update(null, startAt, null, RepeatEndStrategy.Never, RepeatInterval.Monthly, 0, "test");

            var triggers = scheduledJobBuilder
                .BuildTriggers(job)
                .Value;
            triggers[0].StartTimeUtc.Should().BeCloseTo(startAt, 1000);
        }

        [Test]
        public void BiMonthly_Should_Result_In_Two_Triggers()
        {
            var job = DefaultJob;
            var endAt = DateTime.Now.AddDays(1);
            job.Update(null, DateTime.Now.AddMinutes(5), endAt, RepeatEndStrategy.NotUsed, RepeatInterval.BiMonthly, 0, "test");

            var triggers = scheduledJobBuilder
                .BuildTriggers(job)
                .Value;

            triggers.Count.Should().Be(2);
        }

        [Test]
        public void EndAt_On_Trigger_Should_Match_Whats_In_The_Job()
        {
            var job = DefaultJob;
            var endAt = DateTime.Now.AddDays(1);
            job.Update(null, DateTime.Now.AddMinutes(5), endAt, RepeatEndStrategy.OnEndDate, RepeatInterval.Quarterly, 0, "test");

            var triggers = scheduledJobBuilder
                .BuildTriggers(job)
                .Value;

            triggers[0].EndTimeUtc.Should().Be(endAt.ToUniversalTime());
        }

        [Test]
        public void Repeat_Count_In_Message_Sets_End_Date()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddMinutes(5), null, RepeatEndStrategy.AfterOccurrenceNumber, RepeatInterval.BiMonthly, 5, "tester");

            var triggers = scheduledJobBuilder
                .BuildTriggers(job)
                .Value;

            triggers[0].EndTimeUtc.Should().HaveValue();
        }

        [Test]
        public void No_Repeat_End_Strategy_Means_No_EndDate()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddMinutes(5), null, RepeatEndStrategy.Never, RepeatInterval.Never, 0, "test");

            var triggers = scheduledJobBuilder
                .BuildTriggers(job)
                .Value;

            triggers[0].EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void Repeat_Interval_But_No_Repeat_Strategy_Means_Job_Never_Ends()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddMinutes(30), null, RepeatEndStrategy.Never, RepeatInterval.Weekly, 0, "test");

            var trigger = scheduledJobBuilder
                .BuildTriggers(job)
                .Value[0];

            trigger.EndTimeUtc.Should().BeNull();
        }

        [Test]
        public void Repeat_Occurrence_Should_Result_In_Correct_EndDate()
        {
            var job = DefaultJob;
            job.Update(null, Convert.ToDateTime("5/5/2020"), null, RepeatEndStrategy.AfterOccurrenceNumber, RepeatInterval.Monthly, 12, "test");

            var triggers = scheduledJobBuilder.BuildTriggers(job).Value;

            triggers[0].EndTimeUtc.Value.Date.ToShortDateString().Should().Be("4/6/2021");
        }

        [Test]
        public void Trigger_Identity_Should_Be_SubscriptionName_And_JobId()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddHours(1), null, RepeatEndStrategy.Never, RepeatInterval.Never, 0, "test");

            var trigger = scheduledJobBuilder
                .BuildTriggers(job)
                .Value[0] as Quartz.Impl.Triggers.SimpleTriggerImpl;

            trigger.Name.Should().Be(job.JobIdentifier);
            trigger.Group.Should().Be(job.SubscriptionName);
        }

        [Test]
        public void Throw_Exception_If_Scheduled_EndAt_Is_Provided_And_Is_In_The_Past()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(-1), RepeatEndStrategy.Never, RepeatInterval.Never, 0, "test");

            var result = scheduledJobBuilder.BuildTriggers(job);

            result.Error.Should().Contain("EndAt cannot be a date in the past");
        }

        [Test]
        public void Throw_Exception_If_Scheduled_StartAt_Is_In_The_Past_And_Job_Is_Not_Repeating()
        {
            var job = DefaultJob;
            job.Update(null, DateTime.Now.AddMinutes(-5), null, RepeatEndStrategy.Never, RepeatInterval.Never, 0, "test");

            var result = scheduledJobBuilder.BuildTriggers(job);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("StartAt cannot be in the past");
        }

        [Test]
        public void Throw_Exception_If_Repeat_End_At_Is_Provided_But_EndDate_Is_Before_StartDate()
        {
            var job = DefaultJob;
            Action act = () => job.Update(null, DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(3), RepeatEndStrategy.OnEndDate, RepeatInterval.Never, 0, "test");

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("It makes no sense to have an EndDate that's before the StartDate");        }

        [Test]
        public void Throw_Exception_If_RepeatCount_Is_Negative_If_Repeat_Interval_Is_After_Occurrence()
        {
            var job = DefaultJob;

            Action act = () => job.Update(null, DateTime.Now.AddMinutes(5), null, RepeatEndStrategy.AfterOccurrenceNumber, RepeatInterval.Daily, 0, "test");

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage($"{nameof(Job.RepeatOccurrenceNumber)} must be > 0*");
        }
    }
}