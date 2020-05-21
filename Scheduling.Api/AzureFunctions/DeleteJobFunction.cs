using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scheduling.DataAccess.Repositories;
using Scheduling.SharedPackage.Constants;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AzureFunctions
{
    public class DeleteJobFunction
    {
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public DeleteJobFunction(IJobMetaDataRepository jobMetaDataRepo)
        {
            this.jobMetaDataRepo = jobMetaDataRepo;
        }

        [FunctionName("DeleteJob")]
        [return: ServiceBus(MessageQueueNames.Delete, Connection = "ServiceBusConnectionString")]
        public async Task<Message> DeleteJob([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Job/{subscriptionName}/{jobIdentifier}")]
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
                    return new Message(Encoding.UTF8.GetBytes((JsonConvert.SerializeObject(jobLocator))));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to delete job. SubscriptionName: {subscriptionName}, JobIdentifier: {jobIdentifier}");
            }

            return null;
        }
    }
}