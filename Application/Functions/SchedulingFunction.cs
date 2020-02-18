using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Scheduler.Functions
{
    public static class SchedulingFunction
    {
        public static void ScheduleInboundJobs([ServiceBusTrigger("scheduling-inbound")] string message, ILogger logger)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
        }
    }
}
