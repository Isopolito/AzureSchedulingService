using System;

namespace Scheduling.SharedPackage.Messages
{
    public class DeleteJobMessage
    {
        public Guid JobUid { get; set; }
        public string SubscriptionId { get; set; }
    }
}
