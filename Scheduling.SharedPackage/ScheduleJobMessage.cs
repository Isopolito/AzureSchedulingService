using System;

namespace Scheduling.SharedPackage
{
    public class ScheduleJobMessage
    {
        public Guid JobUid { get; set; }
        public string QueueName { get; set; }
        public JobSchedule Schedule { get; set; }
    }
}
