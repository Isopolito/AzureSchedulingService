using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using LogLevel = Quartz.Logging.LogLevel;

namespace Scheduling.Orchestrator.Logging
{
    public class QuartzLoggingProvider : ILogProvider
    {
        private readonly ILogger logger;

        public QuartzLoggingProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func == null) return false;
                var message = string.Join(',', func());

                if (level == LogLevel.Error)
                {
                    logger.LogError(message, parameters);
                }
                else if (level == LogLevel.Warn)
                {
                    logger.LogWarning(message, parameters);
                }
                else if (level == LogLevel.Info)
                {
                    logger.LogInformation(message, parameters);
                }

                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}