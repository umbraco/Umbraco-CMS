using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal sealed class ErrorCounterFilter : ILogFilter
{
    /// <summary>
    /// Gets the count of errors.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Evaluates the specified log event, incrementing the error count if the event is of level Error, Fatal, or contains an exception.
    /// Always returns false to indicate the event should not be added to the log event list.
    /// </summary>
    /// <param name="e">The log event to evaluate.</param>
    /// <returns>False, indicating the event should not be added to the list.</returns>
    public bool TakeLogEvent(LogEvent e)
    {
        if (e.Level == LogEventLevel.Fatal || e.Level == LogEventLevel.Error || e.Exception != null)
        {
            Count++;
        }

        // Don't add it to the list
        return false;
    }
}
