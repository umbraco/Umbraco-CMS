using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

// TODO (V12): Remove this class that's no longer used.

[Obsolete("Please use RecurringHostedServiceBase.GetDelay(). This class is no longer used within Umbraco and will be removed in V12.")]
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

        // Otherwise start at scheduled time according to cron expression, unless within the default delay period.
        DateTime firstRunOccurance = cronTabParser.GetNextOccurrence(firstRunTime, now);
        TimeSpan delay = firstRunOccurance - now;
        return delay < defaultDelay
            ? defaultDelay
            : delay;
    }
}
