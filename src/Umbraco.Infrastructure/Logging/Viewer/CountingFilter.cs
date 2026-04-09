using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal sealed class CountingFilter : ILogFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountingFilter"/> class, which is used to filter and count log entries.
    /// </summary>
    public CountingFilter() => Counts = new LogLevelCounts();

    /// <summary>
    /// Gets an object containing the number of log entries for each log level.
    /// </summary>
    public LogLevelCounts Counts { get; }

    /// <summary>
    /// Increments the count for the specified log event level and always returns false to indicate the event should not be retained.
    /// </summary>
    /// <param name="e">The log event to process and count.</param>
    /// <returns>False, indicating the event should not be added to the log event list.</returns>
    public bool TakeLogEvent(LogEvent e)
    {
        switch (e.Level)
        {
            case LogEventLevel.Debug:
                Counts.Debug++;
                break;

            case LogEventLevel.Information:
                Counts.Information++;
                break;

            case LogEventLevel.Warning:
                Counts.Warning++;
                break;

            case LogEventLevel.Error:
                Counts.Error++;
                break;

            case LogEventLevel.Fatal:
                Counts.Fatal++;
                break;
            case LogEventLevel.Verbose:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Don't add it to the list
        return false;
    }
}
