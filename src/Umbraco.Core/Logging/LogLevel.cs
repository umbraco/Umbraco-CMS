namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Specifies the level of a log event.
/// </summary>
public enum LogLevel
{
    /// <summary>
    ///     Verbose level for the most detailed tracing information.
    /// </summary>
    Verbose,

    /// <summary>
    ///     Debug level for internal system events useful for debugging.
    /// </summary>
    Debug,

    /// <summary>
    ///     Information level for general informational messages.
    /// </summary>
    Information,

    /// <summary>
    ///     Warning level for potentially harmful situations.
    /// </summary>
    Warning,

    /// <summary>
    ///     Error level for error events that might still allow the application to continue running.
    /// </summary>
    Error,

    /// <summary>
    ///     Fatal level for very severe error events that will presumably lead the application to abort.
    /// </summary>
    Fatal,
}
