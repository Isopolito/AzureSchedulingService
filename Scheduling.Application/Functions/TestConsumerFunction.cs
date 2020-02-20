using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Scheduling.Application.Functions
{
    public static class TestConsumerFunction
    {
        public static void TestConsumer([ServiceBusTrigger("scheduling-assessments")] string message, ILogger logger)
        {
        }
    }
}
