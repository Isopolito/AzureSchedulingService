using System.Collections.Generic;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Scheduling
{
    // To build cron expressions
    // https://www.freeformatter.com/cron-expression-generator-quartz.html
    public interface ICronExpressionGenerator
    {
        // A collection of strings is returned here because certain requirements (namely Bi-Monthly require two separate cron expressions)
        IReadOnlyList<string> Create(Job schedule);
    }
}