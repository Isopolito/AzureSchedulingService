using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Quartz;
using Scheduling.SharedPackage.Messages;
using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Jobs.Services
{
    public interface IScheduledJobBuilder
    {
        Result<IReadOnlyList<ITrigger>> BuildTriggers(string jobUid, string subscriptionName, JobSchedule schedule);
        Result<IJobDetail> BuildJob(ScheduleJobMessage scheduleJobMessage);
    }
}