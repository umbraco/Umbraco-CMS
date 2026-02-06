namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Specifies the result type of a health check status.
/// </summary>
public enum StatusResultType
{
    /// <summary>
    ///     The health check completed successfully without any issues.
    /// </summary>
    Success,

    /// <summary>
    ///     The health check completed with a warning condition.
    /// </summary>
    Warning,

    /// <summary>
    ///     The health check completed with an error condition.
    /// </summary>
    Error,

    /// <summary>
    ///     The health check completed with informational status.
    /// </summary>
    Info,
}
