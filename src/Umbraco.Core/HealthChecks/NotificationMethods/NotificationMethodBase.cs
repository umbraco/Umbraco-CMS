using System.Reflection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.HealthChecks.NotificationMethods;

public abstract class NotificationMethodBase : IHealthCheckNotificationMethod
{
    protected NotificationMethodBase(IOptionsMonitor<HealthChecksSettings> healthCheckSettings)
    {
        Type type = GetType();
        HealthCheckNotificationMethodAttribute? attribute = type.GetCustomAttribute<HealthCheckNotificationMethodAttribute>();
        if (attribute == null)
        {
            Enabled = false;
            return;
        }

        IDictionary<string, HealthChecksNotificationMethodSettings> notificationMethods =
            healthCheckSettings.CurrentValue.Notification.NotificationMethods;
        if (!notificationMethods.TryGetValue(
            attribute.Alias, out HealthChecksNotificationMethodSettings? notificationMethod))
        {
            Enabled = false;
            return;
        }

        Enabled = notificationMethod.Enabled;
        FailureOnly = notificationMethod.FailureOnly;
        Verbosity = notificationMethod.Verbosity;
        Settings = notificationMethod.Settings;
    }

    public bool FailureOnly { get; protected set; }

    public HealthCheckNotificationVerbosity Verbosity { get; protected set; }

    public IDictionary<string, string>? Settings { get; }

    public bool Enabled { get; protected set; }

    public abstract Task SendAsync(HealthCheckResults results);

    protected bool ShouldSend(HealthCheckResults results) => Enabled && (!FailureOnly || !results.AllChecksSuccessful);
}
