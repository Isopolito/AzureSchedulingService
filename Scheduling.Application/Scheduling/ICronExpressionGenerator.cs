using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Scheduling
{
    public interface ICronExpressionGenerator
    {
        string Create(JobSchedule jobSchedule);
    }

    // For build cron expressions
    // https://www.freeformatter.com/cron-expression-generator-quartz.html
    public class CronExpressionGenerator : ICronExpressionGenerator
    {
        public string Create(JobSchedule jobSchedule)
        {
            throw new System.NotImplementedException();
        }
    }
}