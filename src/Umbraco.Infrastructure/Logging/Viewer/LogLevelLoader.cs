using System.Collections.ObjectModel;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Infrastructure.Logging.Serilog;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Provides functionality to load and manage log level configurations used by the logging viewer in Umbraco.
/// </summary>
public class LogLevelLoader : ILogLevelLoader
{
    private readonly UmbracoFileConfiguration _umbracoFileConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Logging.Viewer.LogLevelLoader"/> class.
    /// </summary>
    /// <param name="umbracoFileConfig">The configuration object containing settings for Umbraco file-based logging.</param>
    public LogLevelLoader(UmbracoFileConfiguration umbracoFileConfig) => _umbracoFileConfig = umbracoFileConfig;

    /// <summary>
    ///     Retrieves the configured Serilog log level values for the global minimum and the UmbracoFile sink from the configuration file.
    /// </summary>
    /// <returns>
    ///     A read-only dictionary mapping sink names (e.g., "Global", "UmbracoFile") to their configured Serilog log levels.
    /// </returns>
    [Obsolete("Use ILogViewerService.GetLogLevelsFromSinks instead. Scheduled for removal in Umbraco 15.")]
    public ReadOnlyDictionary<string, LogEventLevel?> GetLogLevelsFromSinks()
    {
        var configuredLogLevels = new Dictionary<string, LogEventLevel?>
        {
            { "Global", GetGlobalMinLogLevel() }, { "UmbracoFile", _umbracoFileConfig.RestrictedToMinimumLevel },
        };

        return new ReadOnlyDictionary<string, LogEventLevel?>(configuredLogLevels);
    }

    /// <summary>
    ///     Gets the minimum Serilog log event level configured in the config file.
    /// </summary>
    /// <returns>The configured minimum <see cref="LogEventLevel"/> for Serilog, or <c>null</c> if not set.</returns>
    [Obsolete("Use ILogViewerService.GetGlobalMinLogLevel instead. Scheduled for removal in Umbraco 15.")]
    public LogEventLevel? GetGlobalMinLogLevel()
    {
        LogEventLevel? logLevel = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>().Where(Log.IsEnabled)
            .DefaultIfEmpty(LogEventLevel.Information).Min();
        return logLevel;
    }
}
