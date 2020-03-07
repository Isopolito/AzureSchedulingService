using CSharpFunctionalExtensions;
using Quartz;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public interface IScheduledJobBuilder
    {
        Result<ITrigger> BuildTrigger(string jobUid, string subscriptionName, JobSchedule schedule);
        Result<IJobDetail> BuildJob(ScheduleJobMessage scheduleJobMessage);
    }
}