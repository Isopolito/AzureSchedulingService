using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scheduling.Engine.Constants;

namespace Scheduling.Orchestrator.ServiceBus
{
    internal class ServiceBus : IServiceBus, IDisposable
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

        public async Task PublishEventToTopic(string subscriptionName, string serializedMessageBody)
        {
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(serializedMessageBody))
                {
                    UserProperties = { ["SubscriptionName"] = subscriptionName }
                };

                await topicClient.SendAsync(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to send message to subscription {subscriptionName}. Message: ${serializedMessageBody}");
            }
        }

        public async Task EnsureSubscriptionIsSetup(string subscriptionName)
        {
            try
            {
                // The hash lookup works only when this class is registered in DI as a singleton
                if (subscriptionsThatHaveBeenSetup.Contains(subscriptionName)) return;

                subscriptionsThatHaveBeenSetup.Add(subscriptionName);
                if (!await managementClient.SubscriptionExistsAsync(topicName, subscriptionName))
                {
                    await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicName, subscriptionName), MakeRule(subscriptionName));
                }
                else
                {
                    // The default rule is to accept everything, so delete it and replace it with the subscriptionName filter
                    await managementClient.DeleteRuleAsync(topicName, subscriptionName, "$Default");
                    await managementClient.CreateRuleAsync(topicName, subscriptionName, MakeRule(subscriptionName));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error setting up subscription for subscriptionName: {subscriptionName}");
            }
        }

        private static RuleDescription MakeRule(string subscriptionName)
            => new RuleDescription
              {
                  Filter = new SqlFilter($"{SchedulingConstants.SubscriptionName} = '{subscriptionName}'"),
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