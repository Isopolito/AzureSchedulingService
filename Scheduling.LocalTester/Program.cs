using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Scheduling.SharedPackage;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Extensions;
using Scheduling.SharedPackage.Models;

namespace Scheduling.LocalTester
{
    internal class Program
    {
        private static SubscriptionClient subscriptionClient;

        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var azureConfig = configuration.GetSection("Azure");
            var connectionStringServiceBus = azureConfig["AzureWebJobsServiceBus"];
            var subscriptionName = azureConfig["SubscriptionName"];
            var topicName = azureConfig["TopicName"];
            subscriptionClient = new SubscriptionClient(connectionStringServiceBus, topicName, subscriptionName);

            RegisterOnMessageHandlerAndReceiveMessages();

            // Wire up the API package in DI to test that everything works as expected
            var schedulingApiOptions = new SchedulingApiServiceOptions
            {
                ServiceAddressFetcher = () => configuration["SchedulingBaseUrl"],
                FunctionKeys = new FunctionKeys
                {
                    // Note function keys are not needed when running local
                    GetJob = configuration["GetJobFuncKey"],
                    AddOrUpdateJob = configuration["AddOrUpdateJobFuncKey"],
                    DeleteJob = configuration["DeleteJobFuncKey"],
                },
            };

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSchedulingApi(schedulingApiOptions)
                .BuildServiceProvider();

            var schedulingApi = serviceProvider.GetService<ISchedulingApiService>();
            await TestSchedulingApi(schedulingApi, subscriptionName);
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken ct)
        {
            Console.WriteLine($"Received Execute Job message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}\n");
        }

        // Use this handler to examine the exceptions received on the message pump.
        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);
        }

        private static async Task TestSchedulingApi(ISchedulingApiService apiService, string subscriptionName)
        {
            Console.WriteLine(@"
| *************************************************************************************************|
| Enter one of the following keys to test the scheduler:                                           |
| *************************************************************************************************|
| a - Add or Update a job (will start in 1 minute and will run every minute)                       |
| d - Delete a job                                                                                 |
| g - Get a job                                                                                    |
| l - Load test by calling Add Or Update X number of times                                         |
| p - Pause an existing job                                                                        |
| r - Resume an existing job                                                                       |
| q - Quit                                                                                         |
| -------------------------------------------------------------------------------------------------|
");
            // TODO: Right now this is quick and dirty, it could be refactored and enhanced in many ways
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        Job job;
                        string jobIdentifier;
                        var key = Console.ReadKey(true);

                        switch (key.KeyChar)
                        {
                            case 'q':
                            case 'Q':
                                return;
                            case 'a':
                            case 'A':
                                Console.Write("[UPSERT] Enter Job Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                Console.Write("[UPSERT] How many times to run?: ");
                                var numberOfTimesToRunInput = Console.ReadLine();
                                if (!int.TryParse(numberOfTimesToRunInput, out var numberOfRuns) || numberOfRuns < 1)
                                {
                                    Console.WriteLine("[UPSERT] Must be a valid and positive integer. Aborting command");
                                }
                                else
                                {
                                    job = new Job(subscriptionName, jobIdentifier, "tester");
                                    job.Update("testing domain", DateTime.Now.AddMinutes(1), null, RepeatEndStrategy.NotUsed, RepeatInterval.NotUsed, numberOfRuns, "test", "0 * * ? * *");
                                    job.SetActivationStatus(true, "tester");
                                    await apiService.AddOrUpdateJob(job);
                                }

                                break;
                            case 'd':
                            case 'D':
                                Console.Write("[DELETE] Enter Job Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                await apiService.DeleteJob(new JobLocator(subscriptionName, jobIdentifier));
                                break;
                            case 'g':
                            case 'G':
                                Console.Write("[GET] Enter Job Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                job = await apiService.GetJob(new JobLocator(subscriptionName, jobIdentifier));
                                Console.WriteLine($"Job: {(job == null ? "Does not exist" : JsonConvert.SerializeObject(job))}");
                                break;
                            case 'l':
                            case 'L':
                                Console.Write("[LOAD] Enter Base for Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                Console.Write("[LOAD] How many Add Or Update calls to make? ");
                                var numberOfCallsInput = Console.ReadLine();
                                if (!int.TryParse(numberOfCallsInput, out var numberOfCalls) || numberOfCalls < 1)
                                {
                                    Console.WriteLine("[LOAD] Must be a valid and positive integer. Aborting command");
                                }
                                else
                                {
                                    while (numberOfCalls > 0)
                                    {
                                        job = new Job(subscriptionName, $"{jobIdentifier}-{numberOfCalls--}", "tester");
                                        job.Update($"load testing domain - {numberOfCalls}", DateTime.Now.AddMinutes(1), null, RepeatEndStrategy.NotUsed, RepeatInterval.NotUsed, 0, "load test");
                                        job.SetActivationStatus(true, "load tester");
                                        await apiService.AddOrUpdateJob(job);
                                    }
                                }

                                break;
                            case 'p':
                            case 'P':
                                Console.Write("[PAUSE] Enter Job Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                job = await apiService.GetJob(new JobLocator(subscriptionName, jobIdentifier));
                                job.SetActivationStatus(false, "tester pause");
                                await apiService.AddOrUpdateJob(job);
                                break;
                            case 'r':
                            case 'R':
                                Console.Write("[RESUME] Enter Job Identifier: ");
                                jobIdentifier = Console.ReadLine();
                                job = await apiService.GetJob(new JobLocator(subscriptionName, jobIdentifier));
                                job.SetActivationStatus(true, "tester resume");
                                await apiService.AddOrUpdateJob(job);
                                break;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine($"Bad parameter(s) supplied when creating Job or JobLocator: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unknown Error: {e.Message}");
                    }
                }
            });
        }
    }
}