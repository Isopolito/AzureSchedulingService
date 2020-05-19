using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.Application.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Models;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class CronExpressionGeneratorSpecs
    {
        private CronExpressionGenerator cronExpressionGenerator;

        private static Job DefaultJob => new Job("Subscription", "JobIdentifier", "Tester");

        [SetUp]
        public void Setup()
        {
            cronExpressionGenerator = new CronExpressionGenerator();
        }

        [Test]
        public void Cron_Expression_Override_Is_Used_When_Provided()
        {
            var job = DefaultJob;
            const string cronExpressionOverride = "0 0 17 ? * * *";
            job.Update(null, new DateTime(2022, 5, 5, 17, 0, 0), null, RepeatEndStrategy.NotUsed, RepeatInterval.NotUsed, 0, "test", cronExpressionOverride);

            var cronExpressions = cronExpressionGenerator.Create(job);

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * * *");
        }

        [Test]
        public void Start_At_5_5_2022_With_Repeat_Daily()
        {
            var job = DefaultJob;
            job.Update(null, new DateTime(2022, 5, 5, 17, 0, 0), null, RepeatEndStrategy.Never, RepeatInterval.Daily, 0, "test");

            var cronExpressions = cronExpressionGenerator.Create(job);

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * * *");
        }
        
        [Test]
        public void Start_At_5_5_2022_With_Repeat_Weekly()
        {
            var job = DefaultJob;
            job.Update(null, new DateTime(2022, 5, 5, 17, 0, 0), null, RepeatEndStrategy.Never, RepeatInterval.Weekly, 0, "test");

            var cronExpressions = cronExpressionGenerator.Create(job);

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * THU *");
        }

        [Test]
        public void Start_At_5_5_2022_With_Repeat_BiMonthly()
        {
            var job = DefaultJob;
            job.Update(null, new DateTime(2022, 5, 5, 17, 0, 0), null, RepeatEndStrategy.Never, RepeatInterval.BiMonthly, 0, "test");

            var cronExpressions = cronExpressionGenerator.Create(job);

            cronExpressions.Count.Should().Be(2);
            cronExpressions[0].Should().Be("0 0 17 ? 1/1 THU#1 *");
            cronExpressions[1].Should().Be("0 0 17 ? 1/1 THU#3 *");
        }
    }
}