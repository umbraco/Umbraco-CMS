using System.Collections.ObjectModel;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Infrastructure.Logging.Serilog;

namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogLevelLoader : ILogLevelLoader
{
    private readonly UmbracoFileConfiguration _umbracoFileConfig;

    public LogLevelLoader(UmbracoFileConfiguration umbracoFileConfig) => _umbracoFileConfig = umbracoFileConfig;

    /// <summary>
    ///     Get the Serilog level values of the global minimum and the UmbracoFile one from the config file.
    /// </summary>
    public ReadOnlyDictionary<string, LogEventLevel?> GetLogLevelsFromSinks()
    {
        var configuredLogLevels = new Dictionary<string, LogEventLevel?>
        {
            { "Global", GetGlobalMinLogLevel() }, { "UmbracoFile", _umbracoFileConfig.RestrictedToMinimumLevel },
        };

        return new ReadOnlyDictionary<string, LogEventLevel?>(configuredLogLevels);
    }

    /// <summary>
    ///     Get the Serilog minimum-level value from the config file.
    /// </summary>
    public LogEventLevel? GetGlobalMinLogLevel()
    {
        LogEventLevel? logLevel = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>().Where(Log.IsEnabled)
            .DefaultIfEmpty(LogEventLevel.Information).Min();
        return logLevel;
    }
}
