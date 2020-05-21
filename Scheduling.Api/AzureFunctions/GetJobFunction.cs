using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Scheduling.DataAccess.Repositories;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AzureFunctions
{
    public class GetJobFunction
    {
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public GetJobFunction(IJobMetaDataRepository jobMetaDataRepo)
        {
            this.jobMetaDataRepo = jobMetaDataRepo;
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
                var jobLocator = new JobLocator(subscriptionName, jobIdentifier);
                return await jobMetaDataRepo.Get(jobLocator, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to get job. SubscriptionName {subscriptionName}, JobIdentifier {jobIdentifier}");
            }

            return null;
        }
    }
}