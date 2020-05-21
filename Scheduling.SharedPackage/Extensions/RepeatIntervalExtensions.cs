using Scheduling.SharedPackage.Enums;

namespace Scheduling.SharedPackage.Extensions
{
    public static class RepeatIntervalExtensions
    {
        public static bool IsRepeating(this RepeatInterval repeatInterval)
            => repeatInterval != RepeatInterval.Never && repeatInterval != RepeatInterval.NotUsed;
    }
}
