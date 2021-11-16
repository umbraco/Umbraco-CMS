using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Core.IO;

namespace Umbraco.Core.Logging.Viewer
{
    internal class JsonLogViewer : LogViewerSourceBase
    {
        private readonly string _logsPath;
        private readonly ILogger _logger;

        public JsonLogViewer(ILogger logger, string logsPath = "", string searchPath = "") : base(searchPath)
        {
            if (string.IsNullOrEmpty(logsPath))
                logsPath = IOHelper.MapPath(SystemDirectories.LogFiles);

            _logsPath = logsPath;
            _logger = logger;
        }

        private const int FileSizeCap = 100;

        public override bool CanHandleLargeLogs => false;

        public override bool CheckCanOpenLogs(LogTimePeriod logTimePeriod)
        {
            //Log Directory
            var logDirectory = _logsPath;

            //Number of entries
            long fileSizeCount = 0;

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = GetSearchPattern(day);

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                fileSizeCount += filesForCurrentDay.Sum(x => new FileInfo(x).Length);
            }

            //The GetLogSize call on JsonLogViewer returns the total file size in bytes
            //Check if the log size is not greater than 100Mb (FileSizeCap)
            var logSizeAsMegabytes = fileSizeCount / 1024 / 1024;
            return logSizeAsMegabytes <= FileSizeCap;
        }

        private string GetSearchPattern(DateTime day)
        {
            return $"*{day:yyyyMMdd}*.json";
        }

        protected override IReadOnlyList<LogEvent> GetLogs(LogTimePeriod logTimePeriod, ILogFilter filter, int skip, int take)
        {
            var logs = new List<LogEvent>();

            //Log Directory
            var logDirectory = _logsPath;

            var count = 0;

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = GetSearchPattern(day);

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                //Foreach file we find - open it
                foreach (var filePath in filesForCurrentDay)
                {
                    //Open log file & add contents to the log collection
                    //Which we then use LINQ to page over
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var stream = new StreamReader(fs))
                        {
                            var reader = new LogEventReader(stream);
                            while (TryRead(reader, out var evt))
                            {
                                //We may get a null if log line is malformed
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

        private bool TryRead(LogEventReader reader, out LogEvent evt)
        {
            try
            {
                return reader.TryRead(out evt);
            }
            catch (JsonReaderException ex)
            {
                // As we are reading/streaming one line at a time in the JSON file
                // Thus we can not report the line number, as it will always be 1
                _logger.Error<JsonLogViewer>(ex, "Unable to parse a line in the JSON log file");

                evt = null;
                return true;
            }
        }
    }
}
