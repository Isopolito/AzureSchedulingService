using System.Threading;
using System.Threading.Tasks;
using Scheduling.SharedPackage.Models;

namespace Scheduling.DataAccess.Repositories
{
    // NOTE: this data doesn't actually affect scheduling. That's currently handled by the quartz engine--it manages its own tables
    public interface IJobMetaDataRepository
    {
        /// <summary>
        /// Return null if job can't be found for subscription and job identifier
        /// </summary>
        Task<Job> Get(JobLocator jobLocator, CancellationToken ct = default);

        /// <summary>
        /// Return true if work was done. Return false if `job` exists already and there are no changes to be made
        /// </summary>
        Task<bool> AddOrUpdate(Job job, CancellationToken ct = default);

        /// <summary>
        /// Return true if there is a job to delete. If the job can't be found return false
        /// </summary>
        Task<bool> Delete(JobLocator jobLocator, CancellationToken ct = default);
    }
}
