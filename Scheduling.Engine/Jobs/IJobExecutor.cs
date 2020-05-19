using System.Threading.Tasks;

namespace Scheduling.Engine.Jobs
{
    // NOTE: Must be implemented by code that uses the engine and registered in IoC container
    public interface IJobExecutor
    {
        Task Execute(string subscriptionName, string jobIdentifier);
    }
}
