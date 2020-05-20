using System.Threading.Tasks;
using Scheduling.SharedPackage.Models;

namespace Scheduling.SharedPackage
{
    public interface ISchedulingApiService
    {
        Task AddOrUpdateJob(Job job);
        Task<Job> GetJob(JobLocator jobLocator);
        Task DeleteJob(JobLocator jobLocator);
    }
}
