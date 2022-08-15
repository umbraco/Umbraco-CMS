using System.Collections.ObjectModel;
using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

public interface ILogLevelLoader
{
    /// <summary>
    ///     Get the Serilog level values of the global minimum and the UmbracoFile one from the config file.
    /// </summary>
    ReadOnlyDictionary<string, LogEventLevel?> GetLogLevelsFromSinks();

    /// <summary>
    ///     Get the Serilog minimum-level value from the config file.
    /// </summary>
    LogEventLevel? GetGlobalMinLogLevel();
}
