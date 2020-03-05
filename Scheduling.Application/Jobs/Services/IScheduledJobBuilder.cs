using Quartz;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public interface IScheduledJobBuilder
    {
        ITrigger BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule);
        void AssertInputIsValid(ScheduleJobMessage scheduleJobMessage);
        IJobDetail BuildJob(ScheduleJobMessage scheduleJobMessage);
    }
}