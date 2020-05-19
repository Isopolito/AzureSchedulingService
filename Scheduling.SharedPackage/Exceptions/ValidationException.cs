using System;
using System.Collections.Generic;

namespace Scheduling.SharedPackage.Exceptions
{
    [Serializable]
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(IEnumerable<string> brokenRules) { BrokenRules = brokenRules; }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }

        public IEnumerable<string> BrokenRules { get; set; }
    }
}