using System;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Quartz.Spi;
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
        public void InitiateScheduler(ILogger logger, IJobFactory jobFactory, CancellationToken ct)
        {
            try
            {
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
