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
    public class AddOrUpdateJobFunction
    {
        private readonly ISchedulingActions schedulingActions;
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public AddOrUpdateJobFunction(ISchedulingActions schedulingActions, IJobMetaDataRepository jobMetaDataRepo)
        {
            this.schedulingActions = schedulingActions;
            this.jobMetaDataRepo = jobMetaDataRepo;
        }

        [FunctionName("AddOrUpdateJob")]
        public async Task AddOrUpdateJob([HttpTrigger(AuthorizationLevel.Function, "put", Route = "Job")]
                                HttpRequest req,
                                ILogger logger,
                                CancellationToken ct)
        {
            string requestBody = null;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var job = JsonConvert.DeserializeObject<Job>(requestBody);

                if (await jobMetaDataRepo.AddOrUpdate(job, ct))
                {
                    await schedulingActions.AddOrUpdateJob(job, ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {requestBody}");
            }
        }
    }
}