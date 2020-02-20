using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Scheduling.Application.Functions
{
    public static class InboundMessageFunction
    {
        public static async Task ScheduleInboundJobs([ServiceBusTrigger("scheduling-inbound")] string message, ILogger logger)
        {
        }
    }
}
