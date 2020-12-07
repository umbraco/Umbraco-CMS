// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for healthcheck notification settings.
    /// </summary>
    public class HealthChecksNotificationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether health check notifications are enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value for the first run time of a healthcheck notification in crontab format.
        /// </summary>
        public string FirstRunTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for the period of the healthcheck notification.
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Gets or sets a value for the collection of health check notification methods.
        /// </summary>
        public IDictionary<string, HealthChecksNotificationMethodSettings> NotificationMethods { get; set; } = new Dictionary<string, HealthChecksNotificationMethodSettings>();

        /// <summary>
        /// Gets or sets a value for the collection of health checks that are disabled for notifications.
        /// </summary>
        public IEnumerable<DisabledHealthCheckSettings> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheckSettings>();
    }
}
