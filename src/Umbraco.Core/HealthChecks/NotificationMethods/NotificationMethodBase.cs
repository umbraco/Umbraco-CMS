using System.Reflection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.HealthChecks.NotificationMethods;

/// <summary>
///     Provides a base class for health check notification methods.
/// </summary>
public abstract class NotificationMethodBase : IHealthCheckNotificationMethod
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationMethodBase" /> class.
    /// </summary>
    /// <param name="healthCheckSettings">The health check settings monitor.</param>
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

    /// <summary>
    ///     Gets or sets a value indicating whether notifications should only be sent on failure.
    /// </summary>
    public bool FailureOnly { get; protected set; }

    /// <summary>
    ///     Gets or sets the verbosity level for the notification.
    /// </summary>
    public HealthCheckNotificationVerbosity Verbosity { get; protected set; }

    /// <summary>
    ///     Gets the settings dictionary for this notification method.
    /// </summary>
    public IDictionary<string, string>? Settings { get; }

    /// <inheritdoc />
    public bool Enabled { get; protected set; }

    /// <inheritdoc />
    public abstract Task SendAsync(HealthCheckResults results);

    /// <summary>
    ///     Determines whether a notification should be sent based on the results and configuration.
    /// </summary>
    /// <param name="results">The health check results.</param>
    /// <returns><c>true</c> if a notification should be sent; otherwise, <c>false</c>.</returns>
    protected bool ShouldSend(HealthCheckResults results) => Enabled && (!FailureOnly || !results.AllChecksSuccessful);
}
