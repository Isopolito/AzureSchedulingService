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
using Scheduling.Api.AutoMapper;
using Scheduling.DataAccess.Dto;
using Scheduling.DataAccess.Repositories;
using Scheduling.SharedPackage.Constants;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AzureFunctions
{
    public class AddOrUpdateJobFunction
    {
        private readonly IJobRepository jobRepo;

        public AddOrUpdateJobFunction(IJobRepository jobRepo)
        {
            this.jobRepo = jobRepo;
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

                var jobDto = Mapping.Mapper.Map<Job, JobDto>(job);
                if (await jobRepo.AddOrUpdate(jobDto, ct))
                {
                    return new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(job)));
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