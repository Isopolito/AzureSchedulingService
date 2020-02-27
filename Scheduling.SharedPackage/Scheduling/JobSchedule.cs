using System;

namespace Scheduling.SharedPackage.Scheduling
{
    public class JobSchedule
    {
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int RepeatCount { get; set; }
        public TimeSpan? RepeatInterval { get; set; }

        // If provided, will be used instead of above scheduling properties
        // https://www.freeformatter.com/cron-expression-generator-quartz.html
        public string CronOverride { get; set; }
    }
}