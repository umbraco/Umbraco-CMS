using System.Collections.Generic;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IHealthCheckNotificationSettings
    {
        bool Enabled { get; }
        string FirstRunTime { get; }
        int PeriodInHours { get; }
        IReadOnlyDictionary<string, INotificationMethod> NotificationMethods { get; }
        IEnumerable<IDisabledHealthCheck> DisabledChecks { get; }
    }
}
