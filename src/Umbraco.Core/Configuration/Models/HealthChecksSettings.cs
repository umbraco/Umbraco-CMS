using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.Models
{
    public class HealthChecksSettings
    {
        public IEnumerable<DisabledHealthCheckSettings> DisabledChecks { get; set; } = Enumerable.Empty<DisabledHealthCheckSettings>();

        public HealthChecksNotificationSettings Notification { get; set; } = new HealthChecksNotificationSettings();
    }
}
