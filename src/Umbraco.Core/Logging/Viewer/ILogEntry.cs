namespace Umbraco.Cms.Core.Logging.Viewer;

public interface ILogEntry
{
    /// <summary>
    ///     Gets or sets the time at which the log event occurred.
    /// </summary>
    DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the level of the log event.
    /// </summary>
    LogLevel Level { get; set; }

    /// <summary>
    ///     Gets or sets the message template describing the log event.
    /// </summary>
    string? MessageTemplateText { get; set; }

    /// <summary>
    ///     Gets or sets the message template filled with the log event properties.
    /// </summary>
    string? RenderedMessage { get; set; }

    /// <summary>
    ///     Gets or sets the properties associated with the log event.
    /// </summary>
    IReadOnlyDictionary<string, string?> Properties { get; set; }

    /// <summary>
    ///     Gets or sets an exception associated with the log event, or null.
    /// </summary>
    string? Exception { get; set; }
}
