using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Models;

namespace Scheduling.LocalTester
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var azureConfig = configuration.GetSection("Azure");
            var addJobQueueName = azureConfig["SchedulingAddJobQueueName"];
            var deleteJobQueueName = azureConfig["SchedulingDeleteJobQueueName"];
            var connectionStringServiceBus = azureConfig["AzureWebJobsServiceBus"];
            var subscriptionName = azureConfig["SubscriptionName"];

            await StartSendingMessages(connectionStringServiceBus, addJobQueueName, deleteJobQueueName, subscriptionName);
        }

        private static async Task StartSendingMessages(string connectionStringServiceBus, string addJobQueueName, string deleteJobQueueName, string subscriptionName)
        {
            var addJobQueueClient = new QueueClient(connectionStringServiceBus, addJobQueueName);
            var deleteJobQueueClient = new QueueClient(connectionStringServiceBus, deleteJobQueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to send a message....");
            Console.WriteLine("======================================================");

            while (true)
            {
                Console.ReadKey();

                var jobIdentifier = $"This is a uid - {DateTime.Now}";
                Thread.Sleep(1);
                //await SendDeleteJobMessagesToQueueAsync(deleteJobQueueClient, jobIdentifier);
                await SendAddJobMessagesToQueueAsync(addJobQueueClient, subscriptionName, jobIdentifier);
            }

            //await queueClient.CloseAsync();
        }

        private static async Task SendDeleteJobMessagesToQueueAsync(QueueClient queueClient, string subscriptionName, string jobIdentifier)
        {
            try
            {
                var deleteJobMessage = new JobLocator(subscriptionName, jobIdentifier);
                var messageBody = JsonConvert.SerializeObject(deleteJobMessage);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }

        private static async Task SendAddJobMessagesToQueueAsync(QueueClient queueClient, string subscriptionName, string jobIdentifier)
        {
            try
            {
                var job = new Job(subscriptionName, jobIdentifier, "test");
                job.Update(null, DateTime.Now, DateTime.Now.AddMinutes(15), RepeatEndStrategy.AfterOccurrenceNumber, RepeatInterval.Daily, 99, "test");

                var messageBody = JsonConvert.SerializeObject(job);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }
    }
}
