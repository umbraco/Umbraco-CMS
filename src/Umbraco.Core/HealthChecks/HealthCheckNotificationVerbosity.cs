namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Specifies the verbosity level for health check notifications.
/// </summary>
public enum HealthCheckNotificationVerbosity
{
    /// <summary>
    ///     Provides a brief summary of the health check results.
    /// </summary>
    Summary,

    /// <summary>
    ///     Provides detailed information about the health check results.
    /// </summary>
    Detailed,
}
