using System;
using FluentAssertions;
using Microsoft.Azure.Amqp.Serialization;
using NUnit.Framework;
using Scheduling.Application.Scheduling;
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
        public void Start_At_5_5_2022_With_No_Interval()
        {
            var cronExpression = cronExpressionGenerator.Create(new JobSchedule
            {
                StartAt = Convert.ToDateTime("5/5/2022"),
            });

            cronExpression.Should().Be("0 0 0 5 MAY ? 2022");
        }
    }
}