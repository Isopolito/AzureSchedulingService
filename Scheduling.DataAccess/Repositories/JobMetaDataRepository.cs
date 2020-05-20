using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Scheduling.DataAccess.Contexts;
using Scheduling.SharedPackage.Extensions;
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

        public async Task<Job> Get(JobLocator jobLocator, CancellationToken ct = default)
        {
            var jobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == jobLocator.SubscriptionName && j.JobIdentifier == jobLocator.JobIdentifier, ct);

            return jobEntity != null 
                       ? mapper.Map<Entities.Job, Job>(jobEntity)
                       : null;
        }

        public async Task<bool> AddOrUpdate(Job job, CancellationToken ct = default)
        {
            var jobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == job.SubscriptionName && j.JobIdentifier == job.JobIdentifier, ct);

            // Return false if there are no updates to be made
            if (jobEntity != null
                && jobEntity.DomainName == job.DomainName
                && jobEntity.IsActive == job.IsActive
                && jobEntity.RepeatEndStrategyId == (int) job.RepeatEndStrategy
                && jobEntity.RepeatIntervalId == (int) job.RepeatInterval
                && jobEntity.StartAt.IsEqualToTheMinute(job.StartAt)
                && jobEntity.EndAt.IsEqualToTheMinute(job.EndAt)
                && jobEntity.CronExpressionOverride == job.CronExpressionOverride
                && jobEntity.RepeatOccurrenceNumber == job.RepeatOccurrenceNumber)
            {
                return false;
            }

            jobEntity = mapper.Map<Job, Entities.Job>(job);
            context.Jobs.Attach(jobEntity);
            await context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> Delete(JobLocator jobLocator, CancellationToken ct = default)
        {
            var jobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == jobLocator.SubscriptionName && j.JobIdentifier == jobLocator.JobIdentifier, ct);
            if (jobEntity == null) return false;

            context.Jobs.Remove(jobEntity);
            await context.SaveChangesAsync(ct);

            return true;
        }
    }
}