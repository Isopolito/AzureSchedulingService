using System.Threading.Tasks;
using Scheduling.SharedPackage;

namespace Scheduling.Application.Scheduling
{
    public interface ISchedulingActions
    {
        Task StartScheduler();
        Task AddJob(ScheduleJobMessage scheduleJobMessage);
        Task DeleteJob(ScheduleJobMessage scheduleJobMessage);
        Task UpdateJob(ScheduleJobMessage scheduleJobMessage);
    }
}