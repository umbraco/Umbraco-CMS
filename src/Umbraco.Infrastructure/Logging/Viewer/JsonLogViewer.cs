using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Core.Logging.Viewer
{
    internal class JsonLogViewer : LogViewerSourceBase
    {
        private readonly string _logsPath;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public JsonLogViewer(ILogger logger, ILogViewerConfig logViewerConfig, IHostingEnvironment hostingEnvironment) : base(logViewerConfig)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;

            // TODO: this path is hard coded but it can actually be configured, but that is done via Serilog and we don't have a different abstraction/config
            // for the logging path. We could make that, but then how would we get that abstraction into the Serilog config? I'm sure there is a way but
            // don't have time right now to resolve that (since this was hard coded before). We could have a single/simple ILogConfig for umbraco that purely specifies
            // the logging path and then we can have a special token that we replace in the serilog config that maps to that location? then at least we could inject
            // that config in places where we are hard coding this path.
            _logsPath = Path.Combine(_hostingEnvironment.ApplicationPhysicalPath, @"App_Data\Logs\");
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

            var count = 0;

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = GetSearchPattern(day);

                var filesForCurrentDay = Directory.GetFiles(_logsPath, filesToFind);

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
