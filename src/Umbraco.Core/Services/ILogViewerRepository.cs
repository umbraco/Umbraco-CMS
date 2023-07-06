using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Core.Services;

public interface ILogViewerRepository
{
    IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, string? filterExpression = null);

    LogLevelCounts GetLogCount(LogTimePeriod logTimePeriod);

    LogTemplate[] GetMessageTemplates(LogTimePeriod logTimePeriod);

    LogLevel GetGlobalMinLogLevel();

    LogLevel RestrictedToMinimumLevel();
}
