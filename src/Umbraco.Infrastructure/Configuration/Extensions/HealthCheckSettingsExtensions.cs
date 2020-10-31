using System;
using NCrontab;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Infrastructure.Configuration.Extensions
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
                // Otherwise start at scheduled time according to cron expression, unless within the default delay period.
                var firstRunTimeCronExpression = CrontabSchedule.Parse(firstRunTime);
                var firstRunOccurance = firstRunTimeCronExpression.GetNextOccurrence(now);
                var delay = firstRunOccurance - now;
                return delay < defaultDelay
                    ? defaultDelay
                    : delay;
            }
        }
    }
}
