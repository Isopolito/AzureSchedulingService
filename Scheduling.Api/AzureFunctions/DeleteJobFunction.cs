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
        private readonly IJobRepository jobRepo;

        public DeleteJobFunction(IJobRepository jobRepo)
        {
            this.jobRepo = jobRepo;
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
                if (await jobRepo.Delete(subscriptionName, jobIdentifier, ct))
                {
                    return new Message(Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new JobLocator(subscriptionName, jobIdentifier))));
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