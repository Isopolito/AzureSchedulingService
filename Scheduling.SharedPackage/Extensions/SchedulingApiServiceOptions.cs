using System;
using Scheduling.SharedPackage.Models;

namespace Scheduling.SharedPackage.Extensions
{
    public class SchedulingApiServiceOptions
    {
        public FunctionKeys FunctionKeys { get; set; }
        public Func<string> ServiceAddressFetcher { get; set; }
    }
}
