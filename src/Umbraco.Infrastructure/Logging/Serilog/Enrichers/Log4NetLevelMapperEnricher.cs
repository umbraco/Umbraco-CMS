using Serilog.Core;
using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Serilog.Enrichers;

/// <summary>
///     This is used to create a new property in Logs called 'Log4NetLevel'
///     So that we can map Serilog levels to Log4Net levels - so log files stay consistent
/// </summary>
internal sealed class Log4NetLevelMapperEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the specified log event by adding a <c>Log4NetLevel</c> property with a value mapped from the Serilog log level to a Log4Net-compatible string.
    /// The mapped value is padded to five characters to align log output formatting, matching Log4Net conventions.
    /// </summary>
    /// <param name="logEvent">The log event to enrich. This parameter must not be null.</param>
    /// <param name="propertyFactory">The factory used to create log event properties. This parameter must not be null.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string log4NetLevel;

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
                log4NetLevel =
                    "INFO "; // Padded string so that all log levels are 5 chars long (needed to keep the txt log file lined up nicely)
                break;

            case LogEventLevel.Verbose:
                log4NetLevel =
                    "ALL  "; // Padded string so that all log levels are 5 chars long (needed to keep the txt log file lined up nicely)
                break;

            case LogEventLevel.Warning:
                log4NetLevel =
                    "WARN "; // Padded string so that all log levels are 5 chars long (needed to keep the txt log file lined up nicely)
                break;
            default:
                log4NetLevel = string.Empty;
                break;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Log4NetLevel", log4NetLevel));
    }
}
