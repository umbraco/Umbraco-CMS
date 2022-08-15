using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using ILogger = Serilog.ILogger;

namespace Umbraco.Cms.Core.Logging.Viewer;

internal class SerilogJsonLogViewer : SerilogLogViewerSourceBase
{
    private const int FileSizeCap = 100;
    private readonly ILogger<SerilogJsonLogViewer> _logger;
    private readonly string _logsPath;

    public SerilogJsonLogViewer(
        ILogger<SerilogJsonLogViewer> logger,
        ILogViewerConfig logViewerConfig,
        ILoggingConfiguration loggingConfiguration,
        ILogLevelLoader logLevelLoader,
        ILogger serilogLog)
        : base(logViewerConfig, logLevelLoader, serilogLog)
    {
        _logger = logger;
        _logsPath = loggingConfiguration.LogDirectory;
    }

    public override bool CanHandleLargeLogs => false;

    public override bool CheckCanOpenLogs(LogTimePeriod logTimePeriod)
    {
        // Log Directory
        var logDirectory = _logsPath;

        // Number of entries
        long fileSizeCount = 0;

        // foreach full day in the range - see if we can find one or more filenames that end with
        // yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
        for (DateTime day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
        {
            // Filename ending to search for (As could be multiple)
            var filesToFind = GetSearchPattern(day);

            var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

            fileSizeCount += filesForCurrentDay.Sum(x => new FileInfo(x).Length);
        }

        // The GetLogSize call on JsonLogViewer returns the total file size in bytes
        // Check if the log size is not greater than 100Mb (FileSizeCap)
        var logSizeAsMegabytes = fileSizeCount / 1024 / 1024;
        return logSizeAsMegabytes <= FileSizeCap;
    }

    protected override IReadOnlyList<LogEvent> GetLogs(LogTimePeriod logTimePeriod, ILogFilter filter, int skip,
        int take)
    {
        var logs = new List<LogEvent>();

        var count = 0;

        // foreach full day in the range - see if we can find one or more filenames that end with
        // yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
        for (DateTime day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
        {
            // Filename ending to search for (As could be multiple)
            var filesToFind = GetSearchPattern(day);

            var filesForCurrentDay = Directory.GetFiles(_logsPath, filesToFind);

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

                            if (count > skip + take)
                            {
                                break;
                            }

                            if (count < skip)
                            {
                                count++;
                                continue;
                            }

                            if (filter.TakeLogEvent(evt))
                            {
                                logs.Add(evt);
                            }

                            count++;
                        }
                    }
                }
            }
        }

        return logs;
    }

    private string GetSearchPattern(DateTime day) => $"*{day:yyyyMMdd}*.json";

    private bool TryRead(LogEventReader reader, out LogEvent? evt)
    {
        try
        {
            return reader.TryRead(out evt);
        }
        catch (JsonReaderException ex)
        {
            // As we are reading/streaming one line at a time in the JSON file
            // Thus we can not report the line number, as it will always be 1
            _logger.LogError(ex, "Unable to parse a line in the JSON log file");

            evt = null;
            return true;
        }
    }
}
