using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.Application.Scheduling;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.UnitTests.ScheduledJobs
{
    public class CronExpressionGeneratorSpecs
    {
        private CronExpressionGenerator cronExpressionGenerator;

        [SetUp]
        public void Setup()
        {
            cronExpressionGenerator = new CronExpressionGenerator();
        }

        [Test]
        public void Cron_Expression_Override_Is_Used_When_Provided()
        {
            var cronExpressions = cronExpressionGenerator.Create(new JobSchedule
            {
                StartAt = new DateTime(2022, 5, 5, 17, 0, 0),
                CronExpressionOverride = "0 0 17 ? * * *",
            });

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * * *");
        }

        [Test]
        public void Start_At_5_5_2022_With_Repeat_Daily()
        {
            var cronExpressions = cronExpressionGenerator.Create(new JobSchedule
            {
                StartAt = new DateTime(2022, 5, 5, 17, 0, 0),
                RepeatInterval = RepeatIntervals.Daily,
            });

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * * *");
        }
        
        [Test]
        public void Start_At_5_5_2022_With_Repeat_Weekly()
        {
            var cronExpressions = cronExpressionGenerator.Create(new JobSchedule
            {
                StartAt = new DateTime(2022, 5, 5, 17, 0, 0),
                RepeatInterval = RepeatIntervals.Weekly,
            });

            cronExpressions.Count.Should().Be(1);
            cronExpressions[0].Should().Be("0 0 17 ? * THU *");
        }

        [Test]
        public void Start_At_5_5_2022_With_Repeat_BiMonthly()
        {
            var cronExpressions = cronExpressionGenerator.Create(new JobSchedule
            {
                StartAt = new DateTime(2022, 5, 5, 17, 0, 0),
                RepeatInterval = RepeatIntervals.BiMonthly,
            });

            cronExpressions.Count.Should().Be(2);
            cronExpressions[0].Should().Be("0 0 17 ? 1/1 THU#1 *");
            cronExpressions[1].Should().Be("0 0 17 ? 1/1 THU#3 *");
        }
    }
}