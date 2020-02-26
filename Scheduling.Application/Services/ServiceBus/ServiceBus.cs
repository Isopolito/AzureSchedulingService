using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Constants;

namespace Scheduling.Application.Services.ServiceBus
{
    public class ServiceBus : IServiceBus, IDisposable
    {
        private readonly ILogger<ServiceBus> logger;
        private readonly string topicName;
        private readonly HashSet<string> subscriptionsThatHaveBeenSetup; // This service should be registered as a singleton for this to help
        private readonly TopicClient topicClient;
        private readonly ManagementClient managementClient;

        public ServiceBus(IConfiguration configuration, ILogger<ServiceBus> logger)
        {
            this.logger = logger;
            subscriptionsThatHaveBeenSetup = new HashSet<string>();

            // TODO: Do this so the value comes from the cloud config when not running local
            var serviceBusConnString = configuration.GetValue<string>("AzureWebJobsServiceBus");
            topicName = configuration.GetValue<string>("TopicName");

            topicClient = new TopicClient(serviceBusConnString, topicName);
            managementClient = new ManagementClient(serviceBusConnString);
        }

        public async Task PublishEventToTopic(string subscriptionId, string serializedMessageBody)
        {
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(serializedMessageBody))
                {
                    UserProperties = { ["SubscriptionId"] = subscriptionId }
                };

                await topicClient.SendAsync(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to send message to subscription {subscriptionId}. Message: ${serializedMessageBody}");
            }
        }

        public async Task EnsureSubscriptionIsSetup(string subscriptionId)
        {
            try
            {
                // The hash lookup works only when this class is registered in DI as a singleton
                if (subscriptionsThatHaveBeenSetup.Contains(subscriptionId)) return;

                subscriptionsThatHaveBeenSetup.Add(subscriptionId);
                if (!await managementClient.SubscriptionExistsAsync(topicName, subscriptionId))
                {
                    await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicName, subscriptionId), MakeRule(subscriptionId));
                }
                else
                {
                    // The default rule is to accept everything, so delete it and replace it with the subscriptionId filter
                    await managementClient.DeleteRuleAsync(topicName, subscriptionId, "$Default");
                    await managementClient.CreateRuleAsync(topicName, subscriptionId, MakeRule(subscriptionId));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error setting up subscription for subscriptionId: {subscriptionId}");
            }
        }

        private static RuleDescription MakeRule(string subscriptionId)
            => new RuleDescription
              {
                  Filter = new SqlFilter($"{SchedulingConstants.SubscriptionId} = '{subscriptionId}'"),
              };

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await managementClient.CloseAsync();
                await topicClient.CloseAsync();
            }).Wait(TimeSpan.FromSeconds(10));
        }
    }
}