using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Core.Notifications;

public class HealthCheckCompletedNotification : INotification
{
    public HealthCheckCompletedNotification(HealthCheckResults healthCheckResults)
    {
        HealthCheckResults = healthCheckResults;
    }

    public HealthCheckResults HealthCheckResults { get; }
}
