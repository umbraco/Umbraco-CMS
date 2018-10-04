using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    public class ErrorCounterFilter : ILogFilter
    {
        public int count;

        public bool TakeLogEvent(LogEvent e)
        {
            if (e.Level == LogEventLevel.Fatal || e.Level == LogEventLevel.Error || e.Exception != null)
                count++;

            //Don't add it to the list
            return false;
        }
    }
}
