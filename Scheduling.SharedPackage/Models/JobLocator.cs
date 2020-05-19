namespace Scheduling.SharedPackage.Models
{
    public class JobLocator
    {
        public readonly string JobIdentifier;
        public readonly string SubscriptionName;

        public JobLocator(string subscriptionName, string jobIdentifier)
        {
            JobIdentifier = jobIdentifier;
            SubscriptionName = subscriptionName;
        }
    }
}
