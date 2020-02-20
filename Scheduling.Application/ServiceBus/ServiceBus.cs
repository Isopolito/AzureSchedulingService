using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scheduling.SharedPackage;

namespace Scheduling.Application.ServiceBus
{
    public class ServiceBus : IServiceBus
    {
        private readonly Dictionary<string,QueueClient> queueClients;
        private readonly string serviceBusConnString;

        public ServiceBus()
        {
            queueClients = new Dictionary<string, QueueClient>();

            // TODO: Do this so the value comes from the cloud config when not running local
            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var azureConfig = configuration.GetSection("Azure");

            serviceBusConnString = azureConfig["AzureWebJobsServiceBus"];
        }

        public async Task PublishEvent(string queueName, string jobUid)
        {   
            var queueClient = GetQueueClient(queueName);
            var executeJobMessage = new ExecuteJobMessage
            {
                JobUid = Guid.NewGuid(),
            };
            var messageBody = JsonConvert.SerializeObject(executeJobMessage);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            Console.WriteLine($"Sending message to queue: {messageBody}");

            // Send the message to the queue  
            await queueClient.SendAsync(message);
        }

        private QueueClient GetQueueClient(string queueName)
        {
            if (!queueClients.ContainsKey(queueName))
            {
                queueClients[queueName] = new QueueClient(serviceBusConnString, queueName);
                return queueClients[queueName];
            }

            return queueClients[queueName];
        }
    }
}