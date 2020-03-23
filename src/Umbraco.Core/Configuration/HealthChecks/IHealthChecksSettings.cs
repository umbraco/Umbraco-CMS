using System.Collections.Generic;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IHealthChecksSettings
    {
        IEnumerable<IDisabledHealthCheck> DisabledChecks { get; }
        IHealthCheckNotificationSettings NotificationSettings { get; }
    }
}
