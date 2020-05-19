using System;
using FluentAssertions;
using NUnit.Framework;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Models;

namespace Scheduling.UnitTests
{
    public class JobBuilderSpecs
    {
        [Test]
        public void Throw_Exception_If_SubscriptionName_Is_Missing()
        {
            Action act = () => new Job(null, "unique id 1234", "test");

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage($"The {nameof(Job.SubscriptionName)} is required*");
        }

        [Test]
        public void Throw_Exception_If_JobIdentifier_Is_Missing()
        {
            Action act = () => new Job("sub name", null, "test");

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage($"A {nameof(Job.JobIdentifier)} is required*");
        }

        [Test]
        public void Throw_Exception_If_CreatedBy_Is_Missing()
        {
            Action act = () => new Job("sub name", "ident", null);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("The id of the user *");
        }

        [Test]
        public void Throw_Exception_If_Repeat_End_Strategy_With_No_Interval()
        {
            var job = new Job("sub name", "ident", "test");

            Action act = () => job.Update(null, DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(30), RepeatEndStrategy.OnEndDate, RepeatInterval.Never, 0, "test");

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage($"a {nameof(Job.RepeatEndStrategy)} with no {nameof(Job.RepeatInterval)} doesn't make sense");
        }
    }
}