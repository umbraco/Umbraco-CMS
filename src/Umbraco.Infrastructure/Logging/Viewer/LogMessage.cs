using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogMessage
{
    /// <summary>
    ///     The time at which the log event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     The level of the event.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public LogEventLevel Level { get; set; }

    /// <summary>
    ///     The message template describing the log event.
    /// </summary>
    public string? MessageTemplateText { get; set; }

    /// <summary>
    ///     The message template filled with the log event properties.
    /// </summary>
    public string? RenderedMessage { get; set; }

    /// <summary>
    ///     Properties associated with the log event, including those presented in Serilog.Events.LogEvent.MessageTemplate.
    /// </summary>
    public IReadOnlyDictionary<string, LogEventPropertyValue>? Properties { get; set; }

    /// <summary>
    ///     An exception associated with the log event, or null.
    /// </summary>
    public string? Exception { get; set; }
}
