using System;

namespace Umbraco.Core.Configuration.Models.Extensions
{
    public static class HealthCheckSettingsExtensions
    {
        public static TimeSpan GetNotificationDelay(this HealthChecksSettings settings, DateTime now, TimeSpan defaultDelay)
        {
            // If first run time not set, start with just small delay after application start.
            var firstRunTime = settings.Notification.FirstRunTime;
            if (string.IsNullOrEmpty(firstRunTime))
            {
                return defaultDelay;
            }
            else
            {
                // Otherwise start at scheduled time.
                var delay = TimeSpan.FromMinutes(now.PeriodicMinutesFrom(firstRunTime));
                return (delay < defaultDelay)
                    ? defaultDelay
                    : delay;
            }
        }
    }
}
