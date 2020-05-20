using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.DataAccess.Repositories;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AzureFunctions
{
    public class DeleteJobFunction
    {
        private readonly ISchedulingActions schedulingActions;
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public DeleteJobFunction(ISchedulingActions schedulingActions, IJobMetaDataRepository jobMetaDataRepo)
        {
            this.schedulingActions = schedulingActions;
            this.jobMetaDataRepo = jobMetaDataRepo;
        }

        [FunctionName("DeleteJob")]
        public async Task DeleteJob([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Job/{subscriptionName}/{jobIdentifier}")]
                                    HttpRequest req,
                                    string subscriptionName,
                                    string jobIdentifier,
                                    ILogger logger,
                                    CancellationToken ct)
        {
            try
            {
                var jobLocator = new JobLocator(subscriptionName, jobIdentifier);
                if (await jobMetaDataRepo.Delete(jobLocator, ct))
                {
                    await schedulingActions.DeleteJob(jobLocator, ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to delete job. SubscriptionName: {subscriptionName}, JobIdentifier: {jobIdentifier}");
            }
        }
    }
}