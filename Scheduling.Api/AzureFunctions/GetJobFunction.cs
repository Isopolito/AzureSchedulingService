using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Scheduling.Api.AutoMapper;
using Scheduling.DataAccess.Dto;
using Scheduling.DataAccess.Repositories;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AzureFunctions
{
    public class GetJobFunction
    {
        private readonly IJobRepository jobRepo;

        public GetJobFunction(IJobRepository jobRepo)
        {
            this.jobRepo = jobRepo;
        }

        [FunctionName("GetJob")]
        public async Task<Job> GetJob([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Job/{subscriptionName}/{jobIdentifier}")]
                                      HttpRequest req,
                                      string subscriptionName,
                                      string jobIdentifier,
                                      ILogger logger,
                                      CancellationToken ct)
        {
            try
            {
                var jobDto = await jobRepo.Get(subscriptionName, jobIdentifier, ct);
                return jobDto == null
                           ? null
                           : Mapping.Mapper.Map<JobDto, Job>(jobDto);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to get job. SubscriptionName {subscriptionName}, JobIdentifier {jobIdentifier}");
            }

            return null;
        }
    }
}