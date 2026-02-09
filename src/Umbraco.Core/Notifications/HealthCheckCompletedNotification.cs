// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when health checks have been completed.
/// </summary>
/// <remarks>
///     This notification is published after the health check runner has executed all configured
///     health checks, allowing handlers to process or act on the results.
/// </remarks>
public class HealthCheckCompletedNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckCompletedNotification"/> class.
    /// </summary>
    /// <param name="healthCheckResults">The results of the completed health checks.</param>
    public HealthCheckCompletedNotification(HealthCheckResults healthCheckResults)
    {
        HealthCheckResults = healthCheckResults;
    }

    /// <summary>
    ///     Gets the results of the completed health checks.
    /// </summary>
    public HealthCheckResults HealthCheckResults { get; }
}
