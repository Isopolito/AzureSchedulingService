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
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.AzureFunctions
{
    public class GetJobFunction
    {
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public GetJobFunction(IJobMetaDataRepository jobMetaDataRepo)
        {
            this.jobMetaDataRepo = jobMetaDataRepo;
        }

        // TODO: Handle dead letters
        [FunctionName("Job")]
        public async Task<Job> GetJob([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                                HttpRequest req,
                                ILogger logger,
                                CancellationToken ct)
        {
            string requestBody = null;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var jobLocator = JsonConvert.DeserializeObject<JobLocator>(requestBody);

                return await jobMetaDataRepo.Get(jobLocator, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to get job. Request body: {requestBody}");
            }

            return null;
        }
    }
}