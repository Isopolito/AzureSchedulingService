using System.Threading;
using System.Threading.Tasks;
using Quartz.Spi;
using Scheduling.SharedPackage.Messages;

namespace Scheduling.Application.Services.Scheduling
{
    public interface ISchedulingActions
    {
        Task StartScheduler(IJobFactory jobFactory, CancellationToken ct);
        Task AddOrUpdateJob(ScheduleJobMessage scheduleJobMessage, CancellationToken ct);
        Task DeleteJob(DeleteJobMessage deleteJobMessage, CancellationToken ct);
    }
}