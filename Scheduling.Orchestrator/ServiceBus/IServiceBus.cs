using System.Threading.Tasks;

namespace Scheduling.Orchestrator.ServiceBus
{
    // Register as singleton to allow for caching of queueClients
    public interface IServiceBus
    {
        Task PublishEventToTopic(string subscriptionName, string serializedMessageBody);
        Task EnsureSubscriptionIsSetup(string subscriptionName);
    }
}