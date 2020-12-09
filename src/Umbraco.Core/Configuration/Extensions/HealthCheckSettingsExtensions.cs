using System;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Core.Configuration.Extensions
{
    public static class HealthCheckSettingsExtensions
    {
        public static TimeSpan GetNotificationDelay(this HealthChecksSettings settings, ICronTabParser cronTabParser, DateTime now, TimeSpan defaultDelay)
        {
            // If first run time not set, start with just small delay after application start.
            var firstRunTime = settings.Notification.FirstRunTime;
            if (string.IsNullOrEmpty(firstRunTime))
            {
                return defaultDelay;
            }
            else
            {
                // Otherwise start at scheduled time according to cron expression, unless within the default delay period.
                var firstRunOccurance = cronTabParser.GetNextOccurrence(firstRunTime, now);
                var delay = firstRunOccurance - now;
                return delay < defaultDelay
                    ? defaultDelay
                    : delay;
            }
        }
    }
}
