using Scheduling.SharedPackage.Extensions;

namespace Scheduling.SharedPackage.Models
{
    public class JobLocator : ModelLogicBase
    {
        public readonly string JobIdentifier;
        public readonly string SubscriptionName;

        public JobLocator(string subscriptionName, string jobIdentifier)
        {
            AssertArguments(subscriptionName.HasValue(), $"The {nameof(SubscriptionName)} is required to get a job. This is the messaging topic subscription consumers listen to for their jobs");
            AssertArguments(jobIdentifier.HasValue(), $"A {nameof(JobIdentifier)} is required in order to get a scheduled job");

            JobIdentifier = jobIdentifier;
            SubscriptionName = subscriptionName;
        }
    }
}