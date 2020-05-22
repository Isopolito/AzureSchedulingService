using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Scheduling.Engine.Jobs.Services;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Scheduling
{
    internal class SchedulingActions : ISchedulingActions, IDisposable
    {
        private IScheduler scheduler;
        private readonly ILogger<SchedulingActions> logger;
        private readonly StdSchedulerFactory standardFactory;
        private readonly IScheduledJobBuilder scheduledJobBuilder;
        private readonly IJobFactory jobFactory;

        public SchedulingActions(ILogger<SchedulingActions> logger, IConfiguration configuration, IScheduledJobBuilder scheduledJobBuilder, IJobFactory jobFactory)
        {
            this.logger = logger;
            this.scheduledJobBuilder = scheduledJobBuilder;
            this.jobFactory = jobFactory;

            var quartzSettingsDict = configuration.GetSection("Quartz")
                .GetChildren()
                .ToDictionary(x => x.Key, x => x.Value);

            var quartzSettings = new NameValueCollection();
            foreach (var key in quartzSettingsDict.Keys)
            {
                quartzSettings.Add(key, quartzSettingsDict[key]);
            }
            standardFactory = new StdSchedulerFactory(quartzSettings);
        }

        public async Task StartScheduler(CancellationToken ct)
        {
            // TODO: Get Quartz logging integrated into ILogger
            //LogProvider.SetCurrentLogProvider(new QuartzLoggingProvider(logger));
            scheduler = await standardFactory.GetScheduler(ct);
            scheduler.JobFactory = jobFactory;
            await scheduler.Start(ct);
        }

        public async Task DeleteJob(JobLocator jobLocator, CancellationToken ct)
        {
            await RemoveJobIfAlreadyExists(jobLocator.JobIdentifier, jobLocator.SubscriptionName, ct);
        }

        public async Task AddOrUpdateJob(Job job, CancellationToken ct)
        {
            var jobResult = scheduledJobBuilder.BuildJob(job);
            if (jobResult.IsFailure)
            {
                logger.LogError($"{jobResult.Error}. Message: {job}");
                return;
            }

            var triggerResult = scheduledJobBuilder.BuildTriggers(job);
            if (triggerResult.IsFailure)
            {
                logger.LogError($"{triggerResult.Error}. Message: {job}");
                return;
            }

            await RemoveJobIfAlreadyExists(job.JobIdentifier, job.SubscriptionName, ct);
            if (job.IsActive) await scheduler.ScheduleJob(jobResult.Value, triggerResult.Value, false, ct);
        }

        private async Task RemoveJobIfAlreadyExists(string jobIdentifier, string subscriptionName, CancellationToken ct)
        {
            var jobKey = new JobKey(jobIdentifier, subscriptionName);
            var jobExists = await scheduler.CheckExists(jobKey, ct);
            if (jobExists)
            {
                await scheduler.DeleteJob(jobKey, ct);
            }
        }

        public void Dispose()
        {
            Task.Run(async () => { await scheduler.Shutdown(); }).Wait(TimeSpan.FromSeconds(10));
        }
    }
}