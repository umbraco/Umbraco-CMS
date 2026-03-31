using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

/// <summary>
/// Represents a response model containing information about a logger as returned by the log viewer API.
/// </summary>
public class LoggerResponseModel
{
    /// <summary>
    ///     Gets or sets the name of the log event sink.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the log event level (can be null).
    /// </summary>
    public LogLevel Level { get; set; }
}
