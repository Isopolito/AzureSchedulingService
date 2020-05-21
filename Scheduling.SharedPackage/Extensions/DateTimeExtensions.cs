using System;

namespace Scheduling.SharedPackage.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsEqualToTheMinute(this DateTime self, DateTime otherDateTime)
        {
            var selfNoSeconds = new DateTime(self.Year, self.Month, self.Day, self.Hour, self.Minute, 0);
            var otherNoSeconds = new DateTime(otherDateTime.Year, otherDateTime.Month, otherDateTime.Day, otherDateTime.Hour, otherDateTime.Minute, 0);

            return selfNoSeconds == otherNoSeconds;
        }

        public static bool IsEqualToTheMinute(this DateTime? self, DateTime? otherDateTime)
        {
            if (self == null && otherDateTime == null) return true;
            if (self == null && otherDateTime != null) return false;
            if (self != null && otherDateTime == null) return false;

            return self.Value.IsEqualToTheMinute(otherDateTime.Value);
        }
    }
}
