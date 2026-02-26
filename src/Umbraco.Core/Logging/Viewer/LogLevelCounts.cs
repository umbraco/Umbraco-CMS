namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
///     Represents the count of log entries grouped by log level.
/// </summary>
public class LogLevelCounts
{
    /// <summary>
    ///     Gets or sets the count of information level log entries.
    /// </summary>
    public int Information { get; set; }

    /// <summary>
    ///     Gets or sets the count of debug level log entries.
    /// </summary>
    public int Debug { get; set; }

    /// <summary>
    ///     Gets or sets the count of warning level log entries.
    /// </summary>
    public int Warning { get; set; }

    /// <summary>
    ///     Gets or sets the count of error level log entries.
    /// </summary>
    public int Error { get; set; }

    /// <summary>
    ///     Gets or sets the count of fatal level log entries.
    /// </summary>
    public int Fatal { get; set; }
}
