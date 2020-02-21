using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.SharedPackage;

namespace Scheduling.Application.Functions
{
    public static class TestConsumerFunction
    {
        // This is just for testing locally
        public static void TestConsumer([ServiceBusTrigger("scheduling-execute", "scheduling-pulseassessments-email")] Message message, ILogger logger)
        {
            var executeJob = JsonConvert.DeserializeObject<ExecuteJobMessage>(Encoding.UTF8.GetString(message.Body));
            logger.LogInformation($"Received execute job, message: ${message.Body}");
        }
    }
}
