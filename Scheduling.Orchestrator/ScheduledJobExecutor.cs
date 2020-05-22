using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.DataAccess.Repositories;
using Scheduling.Engine.Jobs;
using Scheduling.Orchestrator.ServiceBus;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Orchestrator
{
    public class ScheduledJobExecutor : IScheduledJobExecutor
    {
        private readonly IServiceBus serviceBus;
        private readonly IJobMetaDataRepository jobMetaDataRepo;
        private readonly ILogger<ScheduledJobExecutor> logger;

        public ScheduledJobExecutor(IServiceBus serviceBus, IJobMetaDataRepository jobMetaDataRepo, ILogger<ScheduledJobExecutor> logger)
        {
            this.serviceBus = serviceBus;
            this.jobMetaDataRepo = jobMetaDataRepo;
            this.logger = logger;
        }

        public async Task Execute(JobLocator jobLocator, bool jobIsCompleted)
        {
            try
            {
                var job = await jobMetaDataRepo.Get(jobLocator);
                if (job == null)
                {
                    logger.LogError($"Job meta data could not be found for SubscriptionName {jobLocator.SubscriptionName} and JobIdentifier {jobLocator.JobIdentifier}. " +
                                    "This should never happen, probably means there's a bug in the scheduler or somebody has manually messed with the scheduling DB");
                    return;
                }

                await serviceBus.EnsureSubscriptionIsSetup(jobLocator.SubscriptionName);
                await serviceBus.PublishEventToTopic(jobLocator.SubscriptionName, JsonConvert.SerializeObject(job));

                if (jobIsCompleted) await jobMetaDataRepo.Delete(jobLocator);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error in JobExecute logic");
            }
        }
    }
}