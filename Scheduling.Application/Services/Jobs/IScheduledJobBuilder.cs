using System;
using Quartz;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Services.Jobs
{
    public interface IScheduledJobBuilder
    {
        ITrigger BuildTrigger(Guid jobUid, string subscriptionId, JobSchedule schedule);
        void AssertInputIsValid(ScheduleJobMessage scheduleJobMessage);
        IJobDetail BuildJob(ScheduleJobMessage scheduleJobMessage);
    }
}