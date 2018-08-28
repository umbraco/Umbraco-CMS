using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public class JsonLogViewer : ILogViewer
    {
        
        public IEnumerable<LogEvent> GetAllLogs(DateTime startDate, DateTime endDate)
        {
            var logs = new List<LogEvent>();
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
                        logs.Add(evt);
                    }
                }
            }

            return logs;
        }

        public int GetNumberOfErrors(DateTime startDate, DateTime endDate)
        {
            var logs = GetAllLogs(startDate, endDate);
            return logs.Count(x => x.Level == LogEventLevel.Fatal && x.Level == LogEventLevel.Error && x.Exception != null);
        }

        public LogLevelCounts GetLogLevelCounts(DateTime startDate, DateTime endDate)
        {
            var logs = GetAllLogs(startDate, endDate);
            return new LogLevelCounts
            {
                Information = logs.Count(x => x.Level == LogEventLevel.Information),
                Debug = logs.Count(x => x.Level == LogEventLevel.Debug),
                Warning = logs.Count(x => x.Level == LogEventLevel.Warning),
                Error = logs.Count(x => x.Level == LogEventLevel.Error),
                Fatal = logs.Count(x => x.Level == LogEventLevel.Fatal)
            };
        }

        public IEnumerable<CommonLogMessage> GetCommonLogMessages(DateTime startDate, DateTime endDate, int numberOfResults)
        {
            var logs = GetAllLogs(startDate, endDate);
            var templates = logs.GroupBy(x => x.MessageTemplate.Text)
                .Select(g => new CommonLogMessage { MessageTemplate = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(numberOfResults);

            return templates;
        }

        public PagedResult<LogMessage> GetLogs(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 100, Direction orderDirection = Direction.Descending)
        {            
            //Get all logs into memory (Not sure this good or not)
            var logs = GetAllLogs(startDate, endDate);
            long totalRecords = logs.Count();
            long pageIndex = pageNumber - 1;
            
            //Skip, Take & Select
            var logItems = logs
                .OrderBy(l => l.Timestamp, orderDirection)
                .Skip(pageSize * pageNumber)
                .Take(pageSize)
                .Select(x => new LogMessage {
                    Timestamp = x.Timestamp,
                    Level = x.Level,
                    MessageTemplateText = x.MessageTemplate.Text,
                    Exception = x.Exception,
                    Properties = x.Properties,
                    RenderedMessage = x.RenderMessage()
                });           

            return new PagedResult<LogMessage>(totalRecords, pageNumber, pageSize)
            {
                Items = logItems
            };            
        }
    }
}
