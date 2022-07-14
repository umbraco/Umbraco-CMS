using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal class ErrorCounterFilter : ILogFilter
{
    public int Count { get; private set; }

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
