using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    public class CountingFilter : ILogFilter
    {
        public CountingFilter()
        {
            Counts = new LogLevelCounts();
        }

        public LogLevelCounts Counts { get; set; }

        public bool TakeLogEvent(LogEvent e)
        {           

            switch (e.Level)
            {
                case LogEventLevel.Verbose:
                    break;

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

                default:
                    break;
            }            

            //Don't add it to the list
            return false;
        }
    }
}
