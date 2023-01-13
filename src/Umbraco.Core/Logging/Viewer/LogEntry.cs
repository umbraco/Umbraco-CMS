namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogEntry : ILogEntry
{
    /// <summary>
    ///     Gets or sets the time at which the log event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the level of the log event.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    ///     Gets or sets the message template describing the log event.
    /// </summary>
    public string? MessageTemplateText { get; set; }

    /// <summary>
    ///     Gets or sets the message template filled with the log event properties.
    /// </summary>
    public string? RenderedMessage { get; set; }

    /// <summary>
    ///     Gets or sets the properties associated with the log event.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Properties { get; set; } = new Dictionary<string, string?>();

    /// <summary>
    ///     Gets or sets an exception associated with the log event, or null.
    /// </summary>
    public string? Exception { get; set; }
}
