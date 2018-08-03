using System.Collections.Generic;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IHealthChecks
    {
        IEnumerable<IDisabledHealthCheck> DisabledChecks { get; }
        IHealthCheckNotificationSettings NotificationSettings { get; }
    }
}