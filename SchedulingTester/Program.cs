﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace SchedulingTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var azureConfig = configuration.GetSection("Azure");
            var queueName = azureConfig["SchedulingQueueName"];
            var connectionStringServiceBus = azureConfig["AzureWebJobsServiceBus"];

                SendMessage(connectionStringServiceBus, queueName).GetAwaiter().GetResult();
        }

        private static async Task SendMessage(string connectionStringServiceBus, string queueName)
        {
            const int numberOfMessages = 5;
            var queueClient = new QueueClient(connectionStringServiceBus, queueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to send messages....");
            Console.WriteLine("======================================================");

            Console.ReadKey();

            // Send Messages  
            await SendMessagesToQueueAsync(queueClient, numberOfMessages);

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();

            await queueClient.CloseAsync();
        }

        static async Task SendMessagesToQueueAsync(QueueClient queueClient, int numberOfMessages)
        {
            try
            {
                for (var i = 0; i < numberOfMessages; i++)
                {
                    // Message that send to the queue  
                    var messageBody = $"SchedulingService POC inbound test:{i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    Console.WriteLine($"Sending message to queue: {messageBody}");

                    // Send the message to the queue  
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }
    }
}
