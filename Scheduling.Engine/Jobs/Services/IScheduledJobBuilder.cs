using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Quartz;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Jobs.Services
{
    public interface IScheduledJobBuilder
    {
        Result<IReadOnlyList<ITrigger>> BuildTriggers(Job job);
        Result<IJobDetail> BuildJob(Job job);
    }
}