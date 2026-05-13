using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Repository for accessing log entries from the Umbraco log files stored on disk.
/// </summary>
public class LogViewerRepository : LogViewerRepositoryBase
{
    private readonly ILoggingConfiguration _loggingConfiguration;
    private readonly ILogger<LogViewerRepository> _logger;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerRepository"/> class.
    /// </summary>
    public LogViewerRepository(
        ILoggingConfiguration loggingConfiguration,
        ILogger<LogViewerRepository> logger,
        IJsonSerializer jsonSerializer,
        UmbracoFileConfiguration umbracoFileConfig)
        : base(umbracoFileConfig)
    {
        _loggingConfiguration = loggingConfiguration;
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc/>
    protected override IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, ILogFilter logFilter)
    {
        var logs = new List<LogEvent>();

        // foreach full day in the range - see if we can find one or more filenames that end with
        // yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
        for (DateTime day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
        {
            // Filename ending to search for (As could be multiple)
            var filesToFind = GetSearchPattern(day);

            var filesForCurrentDay = Directory.GetFiles(_loggingConfiguration.LogDirectory, filesToFind);

            // Foreach file we find - open it. Any failure reading a single file (open error,
            // unrecoverable parse error, etc.) should not prevent the remaining files for the
            // day or date range from being read.
            foreach (var filePath in filesForCurrentDay)
            {
                try
                {
                    ReadLogFile(filePath, logFilter, logs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Skipped log file {FilePath} after a file-level error; the file may be inaccessible or unreadable.",
                        filePath);
                }
            }
        }

        // Order By, Skip, Take & Select
        return logs
            .Select(x => new LogEntry
            {
                Timestamp = x.Timestamp,
                Level = Enum.Parse<LogLevel>(x.Level.ToString()),
                MessageTemplateText = x.MessageTemplate.Text,
                Exception = x.Exception?.ToString(),
                Properties = MapLogMessageProperties(x.Properties),
                RenderedMessage = x.RenderMessage(),
            }).ToArray();
    }

    private void ReadLogFile(string filePath, ILogFilter logFilter, List<LogEvent> logs)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var stream = new StreamReader(fs);
        var reader = new LogEventReader(stream);

        var errorCount = 0;
        Exception? firstError = null;

        while (true)
        {
            LogEvent? evt;
            try
            {
                if (!reader.TryRead(out evt))
                {
                    break;
                }
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException)
            {
                // Serilog.Formatting.Compact.Reader uses Newtonsoft.Json internally and surfaces
                // its exceptions (Umbraco's own serialization is on System.Text.Json, but that
                // doesn't apply here — we have to catch what the reader actually throws).
                // JsonException covers parse failures (e.g. an unterminated string in a truncated
                // entry); InvalidDataException covers structurally-valid JSON that isn't a valid
                // Serilog Compact event. Either way the offending line has been consumed from the
                // underlying StreamReader and the next TryRead call advances. Anything else
                // (IOException, decoder failures, etc.) is propagated to the file-level catch in
                // GetLogs so we don't risk a tight loop or silently swallow a more serious failure.
                errorCount++;
                firstError ??= ex;
                continue;
            }

            // LogEventReader may return true with a null event for a benign skip.
            if (evt is null)
            {
                continue;
            }

            if (logFilter.TakeLogEvent(evt))
            {
                logs.Add(evt);
            }
        }

        if (errorCount > 0)
        {
            _logger.LogWarning(
                firstError,
                "Encountered {ErrorCount} unreadable line(s) while reading log file {FilePath}. The file may contain partially-written or corrupt entries; affected lines were skipped.",
                errorCount,
                filePath);
        }
    }

    private IReadOnlyDictionary<string, string?> MapLogMessageProperties(IReadOnlyDictionary<string, LogEventPropertyValue>? properties)
    {
        var result = new Dictionary<string, string?>();

        if (properties is not null)
        {
            foreach (KeyValuePair<string, LogEventPropertyValue> property in properties)
            {
                string? value;

                if (property.Value is ScalarValue scalarValue)
                {
                    value = scalarValue.Value?.ToString();
                }
                else if (property.Value is StructureValue structureValue)
                {
                    var textWriter = new StringWriter();
                    structureValue.Render(textWriter);
                    value = textWriter.ToString();
                }
                else
                {
                    value = _jsonSerializer.Serialize(property.Value);
                }

                result.Add(property.Key, value);
            }
        }

        return result.AsReadOnly();
    }

    private static string GetSearchPattern(DateTime day) => $"*{day:yyyyMMdd}*.json";
}
