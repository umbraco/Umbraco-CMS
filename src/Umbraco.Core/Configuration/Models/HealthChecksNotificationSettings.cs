using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthChecksNotificationSettings
    {
        public bool Enabled { get; set; } = false;

        public string FirstRunTime { get; set; } = string.Empty;

        public TimeSpan Period { get; set; } = TimeSpan.FromHours(24);

        public IDictionary<string, HealthChecksNotificationMethodSettings> NotificationMethods { get; set; } = new Dictionary<string, HealthChecksNotificationMethodSettings>();

        public IEnumerable<DisabledHealthCheckSettings> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheckSettings>();
    }
}
