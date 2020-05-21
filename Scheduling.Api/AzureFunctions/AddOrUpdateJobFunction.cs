using System;
using System.IO;
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
    public class AddOrUpdateJobFunction
    {
        private readonly IJobMetaDataRepository jobMetaDataRepo;

        public AddOrUpdateJobFunction(IJobMetaDataRepository jobMetaDataRepo)
        {
            this.jobMetaDataRepo = jobMetaDataRepo;
        }

        [FunctionName("AddOrUpdateJob")]
        [return: ServiceBus(MessageQueueNames.AddOrUpdate, Connection = "ServiceBusConnectionString")]
        public async Task<Message> AddOrUpdateJob([HttpTrigger(AuthorizationLevel.Function, "put", Route = "Job")]
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
                    return new Message(Encoding.UTF8.GetBytes((JsonConvert.SerializeObject(job))));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to add or update job. Message: {requestBody}");
            }

            return null;
        }
    }
}