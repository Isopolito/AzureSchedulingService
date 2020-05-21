using System;

namespace Scheduling.SharedPackage.Exceptions
{
    [Serializable]
    public class ServiceException : ApplicationException
    {
        public ServiceException(string message) : base(message) { }
        public ServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}