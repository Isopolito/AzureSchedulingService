using System;

namespace Scheduling.SharedPackage.Scheduling
{
    public class JobSchedule
    {
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int RepeatCount { get; set; }
        public Interval ExecutionInterval { get; set; }
    }
}