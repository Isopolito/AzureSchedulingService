using System;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Quartz.Spi;
using Scheduling.Engine.Scheduling;

namespace Scheduling.Application.AzureFunctions
{
    public class InitiateSchedulerFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public InitiateSchedulerFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        [NoAutomaticTrigger]
        public void InitiateScheduler(ILogger logger, IJobFactory jobFactory, CancellationToken ct)
        {
            try
            {
                // Once the scheduler is running, all actions will be initiated from the functions that listen to the service bus
                logger.LogInformation("Started scheduler");
                schedulingActions.StartScheduler(jobFactory, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to start scheduler");
            }
        }
    }
}
