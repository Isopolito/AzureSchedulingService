using System.Threading;
using System.Threading.Tasks;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Scheduling
{
    public interface ISchedulingActions
    {
        Task StartSchedulerIfNeeded(CancellationToken ct);
        Task AddOrUpdateJob(Job job, CancellationToken ct);
        Task DeleteJob(JobLocator jobLocator, CancellationToken ct);
    }
}