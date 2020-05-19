using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Scheduling.DataAccess.Contexts;
using Scheduling.SharedPackage.Models;

namespace Scheduling.DataAccess.Repositories
{
    internal class JobMetaDataRepository : IJobMetaDataRepository
    {
        private readonly SchedulingContext context;
        private readonly IMapper mapper;

        public JobMetaDataRepository(SchedulingContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<Job> GetJob(string subscriptionName, string jobIdentifier, CancellationToken ct = default)
        {
            var job = await context.Jobs.FirstOrDefaultAsync(j => j.SubscriptionName == subscriptionName && j.JobIdentifier == jobIdentifier, ct);

            return job != null 
                       ? mapper.Map<Entities.Job, Job>(job)
                       : null;
        }

        public Task<bool> AddOrUpdate(Job job, CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> DeleteJob(string subscriptionName, string jobIdentifier, CancellationToken ct = default)
        {
            var job = await context.Jobs.FirstOrDefaultAsync(j => j.SubscriptionName == subscriptionName && j.JobIdentifier == jobIdentifier, ct);
            if (job == null) return false;

            context.Jobs.Remove(job);
            await context.SaveChangesAsync(ct);

            return true;

        }
    }
}