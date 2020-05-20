using System.Threading.Tasks;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Engine.Jobs
{
    /// <summary>
    /// Must be implemented by code that uses the engine and registered in IoC container
    /// </summary>
    public interface IScheduledJobExecutor
    {
        Task Execute(JobLocator jobLocator);
    }
}