using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

public class LogMessageResponseModel
{
    /// <summary>
    ///     Gets or sets the time at which the log event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the level of the event.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    ///     Gets or sets the message template describing the log event (can be null).
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the message template filled with the log event properties (can be null).
    /// </summary>
    public string? RenderedMessage { get; set; }

    /// <summary>
    ///     Gets or sets the properties associated with the log event, including those presented in MessageTemplate.
    /// </summary>
    public IEnumerable<LogMessagePropertyPresentationModel> Properties { get; set; } = Enumerable.Empty<LogMessagePropertyPresentationModel>();

    /// <summary>
    ///     Gets or sets an exception associated with the log event (can be null).
    /// </summary>
    public string? Exception { get; set; }
}
