using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.HealthCheck.Checks;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthChecksSettings
    {
        public IEnumerable<DisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheck>();

        public HealthCheckNotificationSettings NotificationSettings { get; set; } = new HealthCheckNotificationSettings();

        public class HealthCheckNotificationSettings
        {
            public bool Enabled { get; set; } = false;

            public string FirstRunTime { get; set; }

            public int PeriodInHours { get; set; } = 24;

            public IReadOnlyDictionary<string, INotificationMethod> NotificationMethods { get; set; } = new Dictionary<string, INotificationMethod>();

            public IEnumerable<DisabledHealthCheck> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheck>();
        }
    }
}
