using System;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.SharedPackage.Messages
{
    // TODO: Investigate best practices for naming. Past tense?
    public class ScheduleJobMessage
    {
        public string JobUid { get; set; }
        public string SubscriptionName { get; set; }
        public JobSchedule Schedule { get; set; }

        public override string ToString() => $"JobUid: {JobUid}, SubscriptionId: {SubscriptionName}, Schedule: {Schedule}";
    }
}
