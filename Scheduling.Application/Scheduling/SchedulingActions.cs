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
using Scheduling.Application.Constants;
using Scheduling.SharedPackage.Enumerations;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Scheduling
{
    public class SchedulingActions : ISchedulingActions
    {
        private readonly ILogger<SchedulingActions> logger;
        private readonly StdSchedulerFactory factory;
        private IScheduler scheduler;

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
                factory = new StdSchedulerFactory(quartzSettings);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error starting scheduler");
            }
        }

        public async Task StartScheduler(CancellationToken ct)
        {
            try
            {
                scheduler = await factory.GetScheduler(ct);
                await scheduler.Start(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error starting scheduler");
            }
        }

        public async Task DeleteJob(DeleteJobMessage deleteJobMessage, CancellationToken ct)
        {
            if (scheduler == null) await StartScheduler(ct);
            await RemoveJobIfAlreadyExists(deleteJobMessage.JobUid, ct);
        }

        public async Task AddOrUpdateJob(ScheduleJobMessage scheduleJobMessage, CancellationToken ct)
        {
            try
            {
                if (!IsInputValid(scheduleJobMessage)) return;

                if (scheduler == null) await StartScheduler(ct);

                await RemoveJobIfAlreadyExists(scheduleJobMessage.JobUid, ct);

                var job = JobBuilder.Create<ScheduledJob>()
                    .WithIdentity(scheduleJobMessage.JobUid.ToString(), JobConstants.StandardJobGroup)
                    .UsingJobData(JobConstants.JobUid, scheduleJobMessage.JobUid.ToString())
                    .UsingJobData(JobConstants.SubscriptionId, scheduleJobMessage.SubscriptionId)
                    .Build();

                var trigger = BuildTrigger(scheduleJobMessage.Schedule);

                await scheduler.ScheduleJob(job, trigger, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error adding job. ScheduleJobMessage: {JsonConvert.SerializeObject(scheduleJobMessage)}");
            }
        }

        private async Task RemoveJobIfAlreadyExists(Guid jobUid, CancellationToken ct)
        {
            try
            {
                var jobKey = new JobKey(jobUid.ToString(), JobConstants.StandardJobGroup);
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

        private static ITrigger BuildTrigger(JobSchedule schedule)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity(JobConstants.StandardTrigger, JobConstants.StandardTriggerGroup)
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
    }
}
