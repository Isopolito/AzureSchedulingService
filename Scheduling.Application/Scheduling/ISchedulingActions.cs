using System.Threading;
using System.Threading.Tasks;
using Scheduling.SharedPackage.Messages;

namespace Scheduling.Application.Scheduling
{
    public interface ISchedulingActions
    {
        Task StartScheduler(CancellationToken ct);
        Task AddOrUpdateJob(ScheduleJobMessage scheduleJobMessage, CancellationToken ct);
        Task DeleteJob(DeleteJobMessage deleteJobMessage, CancellationToken ct);
    }
}