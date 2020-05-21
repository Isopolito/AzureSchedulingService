using Scheduling.SharedPackage.Enums;

namespace Scheduling.SharedPackage.Extensions
{
    public static class RepeatEndStrategyExtensions
    {
        public static bool HasEndStrategy(this RepeatEndStrategy repeatEndStrategy)
            => repeatEndStrategy != RepeatEndStrategy.Never && repeatEndStrategy != RepeatEndStrategy.NotUsed;

        public static bool DoesNotHaveEndStrategy(this RepeatEndStrategy repeatEndStrategy) => !HasEndStrategy(repeatEndStrategy);
    }
}