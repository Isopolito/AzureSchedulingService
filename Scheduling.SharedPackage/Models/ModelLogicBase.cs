using System;

namespace Scheduling.SharedPackage.Models
{
    public class ModelLogicBase
    {
        public void AssertArguments(bool predicate, string exceptionMessage)
        {
            if (!predicate) throw new ArgumentException(exceptionMessage);
        }
    }
}