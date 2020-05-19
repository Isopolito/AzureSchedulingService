using System.Collections.Generic;
using CronEspresso.NETCore;
using CronEspresso.NETCore.Enums;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Extensions;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.Scheduling
{
    public class CronExpressionGenerator : ICronExpressionGenerator
    {
        public IReadOnlyList<string> Create(Job job)
        {
            if (job.CronExpressionOverride.HasValue())
            {
                return new []{job.CronExpressionOverride};
            }

            if (job.RepeatInterval == RepeatInterval.Daily)
            {
                return new []{CronGenerator.GenerateDailyCronExpression(job.StartAt.TimeOfDay)};
            }

            if (job.RepeatInterval == RepeatInterval.Weekly)
            {
                return new []{CronGenerator.GenerateSetDayCronExpression(job.StartAt.TimeOfDay, job.StartAt.DayOfWeek)};
            }

            if (job.RepeatInterval == RepeatInterval.BiMonthly)
            {
                return new []
                {
                    CronGenerator.GenerateSetDayMonthlyCronExpression(job.StartAt.TimeOfDay, TimeOfMonthToRun.First, job.StartAt.DayOfWeek, 1),
                    CronGenerator.GenerateSetDayMonthlyCronExpression(job.StartAt.TimeOfDay, TimeOfMonthToRun.Third, job.StartAt.DayOfWeek, 1)
                };
            }

            if (job.RepeatInterval == RepeatInterval.Monthly)
            {
                return new []{CronGenerator.GenerateSetDayMonthlyCronExpression(job.StartAt.TimeOfDay, TimeOfMonthToRun.First, job.StartAt.DayOfWeek, 1)};
            }

            if (job.RepeatInterval == RepeatInterval.Quarterly)
            {
                return new []{CronGenerator.GenerateSetDayMonthlyCronExpression(job.StartAt.TimeOfDay, TimeOfMonthToRun.First, job.StartAt.DayOfWeek, 3)};
            }

            return null;
        }
    }
}