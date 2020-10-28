using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.HealthCheck.Checks;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthChecksSettings
    {
        public IEnumerable<DisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheck>();

        public HealthCheckNotificationSettings Notification { get; set; } = new HealthCheckNotificationSettings();

        public class HealthCheckNotificationSettings
        {
            public bool Enabled { get; set; } = false;

            public string FirstRunTime { get; set; } = string.Empty;

            public TimeSpan Period { get; set; } = TimeSpan.FromHours(24);

            public IDictionary<string, HealthCheckNotificationMethodSettings> NotificationMethods { get; set; } = new Dictionary<string, HealthCheckNotificationMethodSettings>();

            public IEnumerable<DisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheck>();
        }
    }
}
