using Scheduling.SharedPackage.Scheduling;

namespace Scheduling.Application.Scheduling
{
    public interface ICronExpressionGenerator
    {
        string Create(JobSchedule schedule);
    }

    // For build cron expressions
    // https://www.freeformatter.com/cron-expression-generator-quartz.html
}