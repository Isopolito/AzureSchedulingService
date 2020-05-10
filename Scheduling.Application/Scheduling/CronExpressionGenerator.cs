using System;
using Scheduling.SharedPackage.Scheduling;
using CronEspresso.NETCore;
using CronEspresso.NETCore.Enums;
using Scheduling.Application.Extensions;
using Scheduling.SharedPackage.Enums;

namespace Scheduling.Application.Scheduling
{
    public class CronExpressionGenerator : ICronExpressionGenerator
    {
        public static readonly int FirstBiMonthlyWeekNumber = 1;
        public static readonly int SecondBiMonthlyWeekNumber = 3;

        public string Create(JobSchedule schedule)
        {
            if (schedule.CronExpressionOverride.HasValue()) return schedule.CronExpressionOverride;

            if (schedule.RepeatInterval == RepeatIntervals.Daily)
            {
                return CronGenerator.GenerateDailyCronExpression(schedule.StartAt.TimeOfDay);
            }

            if (schedule.RepeatInterval == RepeatIntervals.Weekly)
            {
                return CronGenerator.GenerateSetDayCronExpression(schedule.StartAt.TimeOfDay, schedule.StartAt.DayOfWeek);
            }

            if (schedule.RepeatInterval == RepeatIntervals.BiMonthly)
            {
                return BiMonthly(schedule.StartAt);
            }

            if (schedule.RepeatInterval == RepeatIntervals.Monthly)
            {
                return CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.First, schedule.StartAt.DayOfWeek, 1);
            }

            if (schedule.RepeatInterval == RepeatIntervals.Quarterly)
            {
                return CronGenerator.GenerateSetDayMonthlyCronExpression(schedule.StartAt.TimeOfDay, TimeOfMonthToRun.First, schedule.StartAt.DayOfWeek, 3);
            }

            return null;
        }

        private static string BiMonthly(DateTime startAt)
        {
            const string formatter = "* [MIN] [HOUR] [DAYONE],[DAYTWO] * ? *";

            var cronExpression = formatter.Replace("[MIN]", startAt.Minute.ToString());
            cronExpression = cronExpression.Replace("[HOUR]", startAt.Hour.ToString());
            cronExpression = cronExpression.Replace("[DAYONE]", FindDay(startAt.Year, startAt.Month, startAt.DayOfWeek, FirstBiMonthlyWeekNumber).ToString());
            cronExpression = cronExpression.Replace("[DAYTWO]", FindDay(startAt.Year, startAt.Month, startAt.DayOfWeek, SecondBiMonthlyWeekNumber).ToString());

            return cronExpression;
        }

        private static int FindDay(int year, int month, DayOfWeek dayOfWeek, int occurence)
        {
            if (occurence < 1 || occurence > 5)
            {
                throw new ArgumentException("Occurence is invalid");
            }

            var firstDayOfMonth = new DateTime(year, month, 1);

            // Subtract first day of the month with the required day of the week 
            var daysNeeded = (int)dayOfWeek - (int)firstDayOfMonth.DayOfWeek;

            // if it is less than zero we need to get the next week day (add 7 days)
            if (daysNeeded < 0) daysNeeded += 7;

            // dayOfWeek is zero index based; multiply by the occurence to get the day
            var resultedDay = daysNeeded + 1 + 7 * (occurence - 1);

            if (resultedDay > (firstDayOfMonth.AddMonths(1) - firstDayOfMonth).Days)
            {
                throw new Exception($"No {occurence} occurence(s) of {dayOfWeek} in the required month");
            }

            return resultedDay;
        }
    }
}