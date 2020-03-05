using System;

namespace Scheduling.SharedPackage.Messages
{
    public class DeleteJobMessage
    {
        public string JobUid { get; set; }
        public string SubscriptionName { get; set; }
    }
}
