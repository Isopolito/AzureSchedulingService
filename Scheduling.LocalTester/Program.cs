using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scheduling.SharedPackage.Enumerations;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

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

            await StartSendingMessages(connectionStringServiceBus, addJobQueueName, deleteJobQueueName);
        }

        private static async Task StartSendingMessages(string connectionStringServiceBus, string addJobQueueName, string deleteJobQueueName)
        {
            var addJobQueueClient = new QueueClient(connectionStringServiceBus, addJobQueueName);
            var deleteJobQueueClient = new QueueClient(connectionStringServiceBus, deleteJobQueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to send a message....");
            Console.WriteLine("======================================================");

            while (true)
            {
                //Console.ReadKey();

                var guid = Guid.NewGuid();
                Thread.Sleep(1);
                //await SendDeleteJobMessagesToQueueAsync(deleteJobQueueClient, Guid.Parse("4aaf1985-438b-4e8a-baa3-c5aaf7b82f62"));
                await SendAddJobMessagesToQueueAsync(addJobQueueClient, guid);
            }

            //await queueClient.CloseAsync();
        }

        private static async Task SendDeleteJobMessagesToQueueAsync(QueueClient queueClient, Guid guid)
        {
            try
            {
                var deleteJobMessage = new DeleteJobMessage
                {
                    SubscriptionId = "scheduling-testsubscription-1",
                    JobUid = guid,
                };
                var messageBody = JsonConvert.SerializeObject(deleteJobMessage);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }

        private static async Task SendAddJobMessagesToQueueAsync(QueueClient queueClient, Guid guid)
        {
            try
            {
                var jobSchedule = new JobSchedule
                {
                    RepeatCount = 99,
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now.AddMinutes(15),
                    ExecutionInterval = new Interval { IntervalPeriod = IntervalPeriods.Minutes, IntervalValue = 1 }
                };

                var scheduleJobMessage = new ScheduleJobMessage
                {
                    SubscriptionId = "scheduling-testsubscription-1",
                    JobUid = guid,
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
