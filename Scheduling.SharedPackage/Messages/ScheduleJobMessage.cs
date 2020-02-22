using System;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.SharedPackage.Messages
{
    public class ScheduleJobMessage
    {
        public Guid JobUid { get; set; }
        public string SubscriptionId { get; set; }
        public JobSchedule Schedule { get; set; }
    }
}
