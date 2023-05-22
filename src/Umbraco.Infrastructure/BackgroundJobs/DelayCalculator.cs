using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs
{
    public class DelayCalculator
    {
        /// <summary>
        /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optonal
        /// configuration for the first run time is available.
        /// </summary>
        /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
        /// <param name="cronTabParser">An instance of <see cref="ICronTabParser"/></param>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
        /// <returns>The delay before first running the recurring task.</returns>
        public static TimeSpan GetDelay(
            string firstRunTime,
            ICronTabParser cronTabParser,
            ILogger logger,
            TimeSpan defaultDelay) => GetDelay(firstRunTime, cronTabParser, logger, DateTime.Now, defaultDelay);

        /// <summary>
        /// Determines the delay before the first run of a recurring task implemented as a hosted service when an optonal
        /// configuration for the first run time is available.
        /// </summary>
        /// <param name="firstRunTime">The configured time to first run the task in crontab format.</param>
        /// <param name="cronTabParser">An instance of <see cref="ICronTabParser"/></param>
        /// <param name="logger">The logger.</param>
        /// <param name="now">The current datetime.</param>
        /// <param name="defaultDelay">The default delay to use when a first run time is not configured.</param>
        /// <returns>The delay before first running the recurring task.</returns>
        /// <remarks>Internal to expose for unit tests.</remarks>
        internal static TimeSpan GetDelay(
            string firstRunTime,
            ICronTabParser cronTabParser,
            ILogger logger,
            DateTime now,
            TimeSpan defaultDelay)
        {
            // If first run time not set, start with just small delay after application start.
            if (string.IsNullOrEmpty(firstRunTime))
            {
                return defaultDelay;
            }

            // If first run time not a valid cron tab, log, and revert to small delay after application start.
            if (!cronTabParser.IsValidCronTab(firstRunTime))
            {
                logger.LogWarning("Could not parse {FirstRunTime} as a crontab expression. Defaulting to default delay for hosted service start.", firstRunTime);
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
}
