using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Scheduling.Application.Constants;
using Scheduling.SharedPackage;

namespace Scheduling.Application.Jobs
{
    public class SchedulingActions : ISchedulingActions
    {
        private readonly StdSchedulerFactory factory;
        private IScheduler scheduler;

        public SchedulingActions()
        {
            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };

            factory = new StdSchedulerFactory(props);
        }

        public async Task StartScheduler()
        {
            scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        public async Task AddJob(ScheduleJobMessage scheduleJobMessage)
        {
            if (scheduler == null) await StartScheduler();

            // TODO: Assert Guid is not empty in message, log error and bail if so

            // define the job and tie it to our HelloJob class
            var job = JobBuilder.Create<ScheduledJob>()
                .WithIdentity(JobsConstants.IdentityName, JobsConstants.StandardGroup)
                .UsingJobData(JobsConstants.JobUid, scheduleJobMessage.JobUid.ToString())
                .UsingJobData(JobsConstants.QueueName, scheduleJobMessage.QueueName)
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            var trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(40)
                    .RepeatForever())
            .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public Task DeleteJob(ScheduleJobMessage scheduleJobMessage)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateJob(ScheduleJobMessage scheduleJobMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
