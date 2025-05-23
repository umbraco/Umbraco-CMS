using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Core.Services;

public interface ILogViewerRepository
{
    /// <summary>
    ///     Returns the collection of log entries.
    /// </summary>
    IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, string? filterExpression = null);

    /// <summary>
    ///     Returns the number of the different log level entries.
    /// </summary>
    LogLevelCounts GetLogCount(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns a list of all unique message templates and their counts.
    /// </summary>
    LogTemplate[] GetMessageTemplates(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Gets the minimum-level log value from the config file.
    /// </summary>
    LogLevel GetGlobalMinLogLevel();

    /// <summary>
    ///     Get the minimum-level log value from the config file.
    /// </summary>
    LogLevel RestrictedToMinimumLevel();
}
