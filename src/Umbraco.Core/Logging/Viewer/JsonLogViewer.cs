using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Umbraco.Core.Logging.Viewer
{
    public class JsonLogViewer : ILogViewer
    {
        private List<LogEvent> _logs = new List<LogEvent>();

        public JsonLogViewer()
        {
            var filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.DELLBOOK.20180828.json";

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

        public IEnumerable<CommonLogMessage> GetCommonLogMessages(int numberOfResults)
        {
            var templates = _logs.GroupBy(x => x.MessageTemplate.Text)
                .Select(g => new CommonLogMessage { MessageTemplate = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(numberOfResults);

            return templates;
        }

        public IEnumerable<LogMessage> GetLogs()
        {
            var messages = new List<LogMessage>();
            var logs = _logs.Take(20);

            foreach(var log in logs)
            {
                var logItem = new LogMessage
                {
                    Level = log.Level,
                    Properties = log.Properties,
                    Timestamp = log.Timestamp,
                    MessageTemplateText = log.MessageTemplate.Text, //Not necesarily worried about token position just the message text itself
                    RenderedMessage = log.RenderMessage(), //Not returning LogEvent itself from Serilog (as we don't get the rendered txt log back)
                    Exception = log.Exception
                };

                messages.Add(logItem);
            }

            return messages;
        }
    }
}
