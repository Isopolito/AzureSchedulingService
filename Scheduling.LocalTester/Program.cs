using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scheduling.SharedPackage;

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
            var queueName = azureConfig["SchedulingQueueName"];
            var connectionStringServiceBus = azureConfig["AzureWebJobsServiceBus"];

            await SendMessage(connectionStringServiceBus, queueName);
        }

        private static async Task SendMessage(string connectionStringServiceBus, string queueName)
        {
            var queueClient = new QueueClient(connectionStringServiceBus, queueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to send a message....");
            Console.WriteLine("======================================================");

            while (true)
            {
                Console.ReadKey();
                await SendMessagesToQueueAsync(queueClient);
            }

            //await queueClient.CloseAsync();
        }

        private static async Task SendMessagesToQueueAsync(QueueClient queueClient)
        {
            try
            {
                var jobSchedule = new JobSchedule
                {
                    Frequency = 99,
                };

                var scheduleJobMessage = new ScheduleJobMessage
                {
                    SubscriptionId = "scheduling-pulseassessments-email",
                    JobUid = Guid.NewGuid(),
                    Schedule = jobSchedule,
                };
                var messageBody = JsonConvert.SerializeObject(scheduleJobMessage);
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
