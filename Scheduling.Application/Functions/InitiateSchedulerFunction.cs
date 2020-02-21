using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Scheduling;

namespace Scheduling.Application.Functions
{
    public class InitiateSchedulerFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public InitiateSchedulerFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        [NoAutomaticTrigger]
        public void InitiateScheduler(ILogger<InitiateSchedulerFunction> logger)
        {
            logger.LogInformation("Started scheduler");
            schedulingActions.StartScheduler();
        }
    }
}
