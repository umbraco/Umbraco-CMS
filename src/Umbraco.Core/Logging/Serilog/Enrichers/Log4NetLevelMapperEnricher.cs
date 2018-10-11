using Serilog.Core;
using Serilog.Events;

namespace Umbraco.Core.Logging.Serilog.Enrichers
{
    /// <summary>
    /// This is used to create a new property in Logs called 'Log4NetLevel'
    /// So that we can map Serilog levels to Log4Net levels - so log files stay consistent
    /// </summary>
    internal class Log4NetLevelMapperEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var log4NetLevel = string.Empty;

            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                    log4NetLevel = "DEBUG";
                    break;

                case LogEventLevel.Error:
                    log4NetLevel = "ERROR";
                    break;

                case LogEventLevel.Fatal:
                    log4NetLevel = "FATAL";
                    break;

                case LogEventLevel.Information:
                    log4NetLevel = "INFO";
                    break;

                case LogEventLevel.Verbose:
                    log4NetLevel = "ALL";
                    break;

                case LogEventLevel.Warning:
                    log4NetLevel = "WARN";
                    break;
            }

            //Pad string so that all log levels are 5 chars long (needed to keep the txt log file lined up nicely)
            log4NetLevel = log4NetLevel.PadRight(5);

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Log4NetLevel", log4NetLevel));
        }
    }
}
