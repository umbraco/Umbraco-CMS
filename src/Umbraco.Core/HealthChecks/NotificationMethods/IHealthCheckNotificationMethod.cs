using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks.NotificationMethods;

/// <summary>
///     Defines a method for sending health check notifications.
/// </summary>
public interface IHealthCheckNotificationMethod : IDiscoverable
{
    /// <summary>
    ///     Gets a value indicating whether this notification method is enabled.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     Sends the health check results via this notification method.
    /// </summary>
    /// <param name="results">The health check results to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync(HealthCheckResults results);
}
