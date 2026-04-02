using Microsoft.Extensions.Logging;
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

            // Foreach file we find - open it
            foreach (var filePath in filesForCurrentDay)
            {
                // Open log file & add contents to the log collection
                // Which we then use LINQ to page over
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var stream = new StreamReader(fs))
                    {
                        var reader = new LogEventReader(stream);
                        while (TryRead(reader, out LogEvent? evt))
                        {
                            // We may get a null if log line is malformed
                            if (evt == null)
                            {
                                continue;
                            }

                            if (logFilter.TakeLogEvent(evt))
                            {
                                logs.Add(evt);
                            }
                        }
                    }
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

    private bool TryRead(LogEventReader reader, out LogEvent? evt)
    {
        try
        {
            return reader.TryRead(out evt);
        }
        catch (Exception ex)
        {
            // As we are reading/streaming one line at a time in the JSON file
            // Thus we can not report the line number, as it will always be 1
            _logger.LogError(ex, "Unable to parse a line in the JSON log file");

            evt = null;
            return true;
        }
    }
}
