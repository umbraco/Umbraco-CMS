using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents a filter that determines which log entries are included or excluded.
/// </summary>
public interface ILogFilter
{
    /// <summary>
    /// Determines whether the specified log event should be taken (accepted) by the filter.
    /// </summary>
    /// <param name="e">The log event to evaluate.</param>
    /// <returns><c>true</c> if the log event is accepted by the filter; otherwise, <c>false</c>.</returns>
    bool TakeLogEvent(LogEvent e);
}
