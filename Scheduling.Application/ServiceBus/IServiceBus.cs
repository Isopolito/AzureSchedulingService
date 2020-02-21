using System;
using System.Threading.Tasks;

namespace Scheduling.Application.ServiceBus
{
    // Register as singleton to allow for caching of queueClients
    public interface IServiceBus
    {
        Task PublishEventToTopic(string subscriptionId, string serializedMessageBody);
        Task EnsureSubscriptionIsSetup(string subscriptionId);
    }
}