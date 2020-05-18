using System;
using Scheduling.SharedPackage.Enums;

namespace Scheduling.SharedPackage.Scheduling
{
    public class JobSchedule
    {
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int RepeatCount { get; set; }
        public RepeatIntervals RepeatInterval { get; set; }
        public RepeatEndStrategy RepeatEndStrategy { get; set; }
        public string CronExpressionOverride { get; set; }

        public override string ToString() => $"StartAt: {StartAt}, EndAt: {EndAt}, RepeatCount: {RepeatCount}, RepeatInterval: {RepeatInterval}, CronExpressionOverride: {CronExpressionOverride }";
    }
}