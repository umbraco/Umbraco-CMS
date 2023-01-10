using Serilog.Events;

namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

public class LoggerViewModel
{
    /// <summary>
    ///     Gets or sets the name of the log event sink.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the log event level (can be null).
    /// </summary>
    public LogEventLevel? Level { get; set; }
}
