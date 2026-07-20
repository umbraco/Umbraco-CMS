using System.Collections.ObjectModel;
using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents an interface for loading log level configuration settings.
/// </summary>
[Obsolete("Use ILogViewerService instead. Scheduled for removal in Umbraco 15.")]
public interface ILogLevelLoader
{
    /// <summary>
    ///     Retrieves the configured Serilog log event levels for each log sink, including the global minimum and UmbracoFile sinks, from the configuration file.
    /// </summary>
    /// <returns>A read-only dictionary mapping sink names to their corresponding Serilog log event levels, or <c>null</c> if not set.</returns>
    ReadOnlyDictionary<string, LogEventLevel?> GetLogLevelsFromSinks();

    /// <summary>
    /// Gets the global Serilog minimum log level value from the configuration file.
    /// </summary>
    /// <returns>
    /// The globally configured minimum <see cref="LogEventLevel"/>, or <c>null</c> if not set.
    /// </returns>
    LogEventLevel? GetGlobalMinLogLevel();
}
