using System;

namespace Scheduling.DataAccess.Entities
{
    internal class Job
    {
        public int JobId { get; set; }
        public string JobIdentifier { get; set; }
        public string SubscriptionName { get; set; }
        public string DomainName { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int RepeatEndStrategyId { get; set; }
        public int RepeatIntervalId { get; set; }
        public int RepeatOccurrenceNumber { get; set; }
        public string CronExpressionOverride { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual RepeatEndStrategy RepeatEndStrategy { get; set; }
        public virtual RepeatInterval RepeatInterval { get; set; }
    }
}