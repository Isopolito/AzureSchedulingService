using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Application.Functions
{
    public static class ScheduleWatcherFunction
    {
        [NoAutomaticTrigger]
        public static void WatchForJobsToExecute(ILogger logger)
        {
            var i = 0;
            while (true)
            {
                logger.LogInformation($"Hi from WatchSchedule: {i++}");
                Thread.Sleep(2000);
            }
        }
    }
}
