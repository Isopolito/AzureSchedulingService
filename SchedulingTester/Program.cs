using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace SchedulingTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string connectionStringServiceBus = "Endpoint=sb://austin.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=de5FAUcFSnc9KlXWVpxuGqIH2Dpqjq+kkKocpzvs3os=";
            const string queueName = "scheduling-inbound";

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
