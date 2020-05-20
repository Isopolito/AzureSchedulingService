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
        public async Task DeleteJob([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)]
                                HttpRequest req,
                                ILogger logger,
                                CancellationToken ct)
        {
            string requestBody = null;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var jobLocator = JsonConvert.DeserializeObject<JobLocator>(requestBody);

                if (await jobMetaDataRepo.Delete(jobLocator, ct))
                {
                    await schedulingActions.DeleteJob(jobLocator, ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to delete job. Message: {requestBody}");
            }
        }
    }
}