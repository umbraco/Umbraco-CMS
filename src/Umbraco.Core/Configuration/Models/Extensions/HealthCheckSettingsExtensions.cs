using System;

namespace Umbraco.Core.Configuration.Models.Extensions
{
    public static class HealthCheckSettingsExtensions
    {
        public static int GetNotificationPeriodInMilliseconds(this HealthChecksSettings settings)
        {
            return settings.Notification.PeriodInHours * 60 * 60 * 1000;
        }

        public static int GetNotificationDelayInMilliseconds(this HealthChecksSettings settings, DateTime now, int defaultDelayInMilliseconds)
        {
            // If first run time not set, start with just small delay after application start.
            var firstRunTime = settings.Notification.FirstRunTime;
            if (string.IsNullOrEmpty(firstRunTime))
            {
                return defaultDelayInMilliseconds;
            }
            else
            {
                // Otherwise start at scheduled time.
                var delayInMilliseconds = now.PeriodicMinutesFrom(firstRunTime) * 60 * 1000;
                if (delayInMilliseconds < defaultDelayInMilliseconds)
                {
                    return defaultDelayInMilliseconds;
                }

                return delayInMilliseconds;
            }
        }
    }
}
