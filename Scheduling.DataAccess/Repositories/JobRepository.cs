using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Scheduling.DataAccess.AutoMapper;
using Scheduling.DataAccess.Contexts;
using Scheduling.DataAccess.Dto;
using Scheduling.DataAccess.Entities;
using Scheduling.DataAccess.Extensions;

namespace Scheduling.DataAccess.Repositories
{
    internal class JobRepository : IJobRepository
    {
        private readonly SchedulingContext context;

        public JobRepository(SchedulingContext context)
        {
            this.context = context;
        }

        public async Task<JobDto> Get(string subscriptionName, string jobIdentifier, CancellationToken ct = default)
        {
            var jobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == subscriptionName && j.JobIdentifier == jobIdentifier, ct);

            return jobEntity != null 
                       ? Mapping.Mapper.Map<Job, JobDto>(jobEntity)
                       : null;
        }

        public async Task<bool> AddOrUpdate(JobDto jobDto, CancellationToken ct = default)
        {
            var existingJobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == jobDto.SubscriptionName && j.JobIdentifier == jobDto.JobIdentifier, ct);

            // Return false if there are no updates to be made
            if (existingJobEntity != null
                && existingJobEntity.DomainName == jobDto.DomainName
                && existingJobEntity.IsActive == jobDto.IsActive
                && existingJobEntity.RepeatEndStrategyId == jobDto.RepeatEndStrategyId
                && existingJobEntity.RepeatIntervalId == jobDto.RepeatIntervalId
                && existingJobEntity.StartAt.IsEqualToTheMinute(jobDto.StartAt)
                && existingJobEntity.EndAt.IsEqualToTheMinute(jobDto.EndAt)
                && existingJobEntity.CronExpressionOverride == jobDto.CronExpressionOverride
                && existingJobEntity.RepeatOccurrenceNumber == jobDto.RepeatOccurrenceNumber)
            {
                return false;
            }

            var updatedJobEntity = Mapping.Mapper.Map<JobDto, Job>(jobDto);
            if (existingJobEntity != null)
            {
                updatedJobEntity.JobId = existingJobEntity.JobId;
                context.Entry(existingJobEntity).State = EntityState.Detached;
                context.Jobs.Update(updatedJobEntity);
            }
            else
            {
                await context.Jobs.AddAsync(updatedJobEntity, ct);
            }

            await context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> Delete(string subscriptionName, string jobIdentifier, CancellationToken ct = default)
        {
            var jobEntity = await context.Jobs
                          .FirstOrDefaultAsync(j => j.SubscriptionName == subscriptionName && j.JobIdentifier == jobIdentifier, ct);
            if (jobEntity == null) return false;

            context.Jobs.Remove(jobEntity);
            await context.SaveChangesAsync(ct);

            return true;
        }
    }
}