using System.Threading;
using System.Threading.Tasks;
using Quartz.Spi;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.Scheduling
{
    public interface ISchedulingActions
    {
        Task StartScheduler(IJobFactory jobFactory, CancellationToken ct);
        Task AddOrUpdateJob(Job job, CancellationToken ct);
        Task DeleteJob(JobLocator deleteJobModel, CancellationToken ct);
    }
}