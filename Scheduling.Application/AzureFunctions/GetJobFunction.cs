using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Scheduling.Engine.Scheduling;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Application.AzureFunctions
{
    public class GetJobFunction
    {
        private readonly ISchedulingActions schedulingActions;

        public GetJobFunction(ISchedulingActions schedulingActions)
        {
            this.schedulingActions = schedulingActions;
        }

        // TODO: Handle dead letters
        [FunctionName("Job")]
        public Task<Job> GetJob([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
                                HttpRequest req,
                                ILogger logger,
                                ExecutionContext context)
        {
            string body = null;
            try
            {
                // Get Job Logic
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to get job. Message: {body}");
            }

            return null;
        }
    }
}