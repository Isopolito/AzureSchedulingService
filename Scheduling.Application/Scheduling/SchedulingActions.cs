using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;
using Scheduling.Application.Constants;
using Scheduling.Application.Logging;
using Scheduling.SharedPackage.Enumerations;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Scheduling
{
    public class SchedulingActions : ISchedulingActions, IDisposable
    {
        private IScheduler scheduler;
        private readonly ILogger<SchedulingActions> logger;
        private readonly StdSchedulerFactory standardFactory;

        public SchedulingActions(ILogger<SchedulingActions> logger, IConfiguration configuration)
        {
            try
            {
                this.logger = logger;

                var quartzSettingsDict = configuration.GetSection("Quartz")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value);

                var quartzSettings = new NameValueCollection();
                foreach (var (key, value) in quartzSettingsDict) quartzSettings.Add(key, value);
                standardFactory = new StdSchedulerFactory(quartzSettings);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error getting quartz settings");
                throw;
            }
        }

        public async Task StartScheduler(IJobFactory jobFactory, CancellationToken ct)
        {
            try
            {
                // TODO: Get Quartz logging integrated into ILogger
                LogProvider.SetCurrentLogProvider(new QuartzLoggingProvider(logger));
                scheduler = await standardFactory.GetScheduler(ct);
                scheduler.JobFactory = jobFactory;
                await scheduler.Start(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error starting scheduler");
                throw;
            }
        }

        public async Task DeleteJob(DeleteJobMessage deleteJobMessage, CancellationToken ct)
        {
            await RemoveJobIfAlreadyExists(deleteJobMessage.JobUid, deleteJobMessage.SubscriptionId, ct);
        }

        public async Task AddOrUpdateJob(ScheduleJobMessage scheduleJobMessage, CancellationToken ct)
        {
            try
            {
                if (!IsInputValid(scheduleJobMessage)) return;

                await RemoveJobIfAlreadyExists(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionId, ct);

                var job = JobBuilder.Create<ScheduledJob>()
                    .WithIdentity(scheduleJobMessage.JobUid.ToString(), scheduleJobMessage.SubscriptionId)
                    .UsingJobData(SchedulingConstants.JobUid, scheduleJobMessage.JobUid.ToString())
                    .UsingJobData(SchedulingConstants.SubscriptionId, scheduleJobMessage.SubscriptionId)
                    .Build();

                var trigger = BuildTrigger(scheduleJobMessage.JobUid, scheduleJobMessage.SubscriptionId, scheduleJobMessage.Schedule);

                await scheduler.ScheduleJob(job, trigger, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error adding job. ScheduleJobMessage: {JsonConvert.SerializeObject(scheduleJobMessage)}");
            }
        }

        private async Task RemoveJobIfAlreadyExists(Guid jobUid, string subscriptionId, CancellationToken ct)
        {
            try
            {
                var jobKey = new JobKey(jobUid.ToString(), subscriptionId);
                var jobExists = await scheduler.CheckExists(jobKey, ct);
                if (jobExists)
                {
                    await scheduler.DeleteJob(jobKey, ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error deleting scheduled job, JobUid: {jobUid}");
            }
        }

        private bool IsInputValid(ScheduleJobMessage scheduleJobMessage)
        {
            if (scheduleJobMessage.Schedule?.ExecutionInterval == null)
            {
                logger.LogError($"Schedule configuration, including ExecutionInterval, is required. Message: {JsonConvert.SerializeObject(scheduleJobMessage)}");
                return false;
            }

            if (scheduleJobMessage.JobUid == Guid.Empty || string.IsNullOrEmpty(scheduleJobMessage.SubscriptionId))
            {
                logger.LogError($"JobUid and SubscriptionId are required for scheduling a job. Message: {JsonConvert.SerializeObject(scheduleJobMessage)}");
                return false;
            }

            return true;
        }

        private ITrigger BuildTrigger(Guid jobUid, string subscriptionId, JobSchedule schedule)
        {
            try
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(jobUid.ToString(), subscriptionId)
                    .StartAt(schedule.StartAt);

                if (schedule.EndAt.HasValue)
                {
                    trigger.EndAt(schedule.EndAt);
                }

                var simpleSchedule = SimpleScheduleBuilder.Create();
                if (schedule.RepeatCount > 0)
                {
                    simpleSchedule.WithRepeatCount(schedule.RepeatCount);
                }

                var intervalMultiplierInMinutes = 1;
                if (schedule.ExecutionInterval.IntervalPeriod == IntervalPeriods.Hours)
                {
                    intervalMultiplierInMinutes = 60;
                }
                else if (schedule.ExecutionInterval.IntervalPeriod == IntervalPeriods.Days)
                {
                    intervalMultiplierInMinutes = 60 * 24;
                }

                simpleSchedule.WithIntervalInMinutes(schedule.ExecutionInterval.IntervalValue * intervalMultiplierInMinutes);
                trigger.WithSchedule(simpleSchedule);

                return trigger.Build();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to create trigger for jobUid {jobUid}, subscriptionId {subscriptionId}. Schedule: {JsonConvert.SerializeObject(schedule)}");
                throw;
            }
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await scheduler.Shutdown();
            }).Wait(TimeSpan.FromSeconds(10));
        }
    }
}
