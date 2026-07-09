namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
///     Represents a log message template with its occurrence count.
/// </summary>
public class LogTemplate
{
    /// <summary>
    ///     Gets or sets the message template string.
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the number of times this template appears in the logs.
    /// </summary>
    public int Count { get; set; }
}
