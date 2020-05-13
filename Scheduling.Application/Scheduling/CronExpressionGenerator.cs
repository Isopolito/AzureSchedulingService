using System.Collections.Generic;
using Scheduling.SharedPackage.Scheduling;
using CronEspresso.NETCore;
using CronEspresso.NETCore.Enums;
using Scheduling.Application.Extensions;
using Scheduling.SharedPackage.Enums;

namespace Scheduling.Application.Scheduling
{
    public class CronExpressionGenerator : ICronExpressionGenerator
    {
        public IReadOnlyList<string> Create(JobSchedule schedule)
        {
            if (schedule.CronExpressionOverride.HasValue())
            {
                return new []{schedule.CronExpressionOverride};
            }

            if (schedule.RepeatInterval == RepeatIntervals.Daily)
            {
                return new []{CronGenerator.GenerateDailyCronExpression(schedule.StartAt.TimeOfDay)};
            }

            if (schedule.RepeatInterval == RepeatIntervals.Weekly)
            {
                return new []{CronGenerator.GenerateSetDayCronExpression(schedule.StartAt.TimeOfDay, schedule.StartAt.DayOfWeek)};
            }

            if (schedule.RepeatInterval == RepeatIntervals.BiMonthly)
            {
                return new []
                {
                    CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.First, schedule.StartAt.DayOfWeek, 1),
                    CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.Third, schedule.StartAt.DayOfWeek, 1)
                };
            }

            if (schedule.RepeatInterval == RepeatIntervals.Monthly)
            {
                return new []{CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.First, schedule.StartAt.DayOfWeek, 1)};
            }

            if (schedule.RepeatInterval == RepeatIntervals.Quarterly)
            {
                return new []{CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.First, schedule.StartAt.DayOfWeek, 3)};
            }

            return null;
        }
    }
}