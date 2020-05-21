using System;

namespace Scheduling.SharedPackage.Exceptions
{
    [Serializable]
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException(string uri) : base($"User does not have access to {uri}") { }
    }
}