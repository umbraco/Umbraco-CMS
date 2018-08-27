using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Umbraco.Core.Logging.Viewer
{
    public class LogViewer : ILogViewer
    {
        private List<LogEvent> _logs;

        public LogViewer()
        {
            var filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.DELLBOOK.20180824.json";

            //Open log file
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var stream = new StreamReader(fs))
                {
                   
                    var reader = new LogEventReader(stream);
                    LogEvent evt;
                    while (reader.TryRead(out evt))
                    {
                        _logs.Add(evt);
                    }
                }
            }
        }

        public int GetNumberOfErrors()
        {
            return _logs.Count(x => x.Level == LogEventLevel.Fatal && x.Level == LogEventLevel.Error && x.Exception != null);
        }

        public LogLevelCounts GetLogLevelCounts()
        {
            return new LogLevelCounts
            {
                Information = _logs.Count(x => x.Level == LogEventLevel.Information),
                Debug = _logs.Count(x => x.Level == LogEventLevel.Debug),
                Warning = _logs.Count(x => x.Level == LogEventLevel.Warning),
                Error = _logs.Count(x => x.Level == LogEventLevel.Error),
                Fatal = _logs.Count(x => x.Level == LogEventLevel.Fatal)
            };
        }

        public IEnumerable<CommonLogMessage> GetCommonLogMessages()
        {
            var messages = new List<CommonLogMessage>();
            return messages;

        }

        public IEnumerable<LogEvent> GetLogs()
        {
            throw new NotImplementedException();
        }
    }
}
