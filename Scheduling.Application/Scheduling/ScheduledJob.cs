using System.Threading.Tasks;
using Quartz;
using Scheduling.Application.Constants;
using Scheduling.Application.ServiceBus;

namespace Scheduling.Application.Scheduling
{
    public class ScheduledJob : IJob
    {
        private readonly IServiceBus serviceBus;

        public ScheduledJob(IServiceBus serviceBus)
        {
            this.serviceBus = serviceBus;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var subscriptionId = dataMap.GetString(JobsConstants.SubscriptionId);
            var jobUid = dataMap.GetString(JobsConstants.JobUid);

            await serviceBus.PublishEventToTopic(subscriptionId, jobUid);

            var key = context.JobDetail.Key;
            //await Console.Error.WriteLineAsync("Instance " + key + " of DumbJob says: " + jobSays + ", and val is: " + myFloatValue);

        }
    }
}
