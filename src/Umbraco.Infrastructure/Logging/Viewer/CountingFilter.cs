using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal class CountingFilter : ILogFilter
{
    public CountingFilter() => Counts = new LogLevelCounts();

    public LogLevelCounts Counts { get; }

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
