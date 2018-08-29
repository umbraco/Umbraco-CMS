using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Filters.Expressions;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public class JsonLogViewer : ILogViewer
    {
        
        public IEnumerable<LogEvent> GetAllLogs(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var logs = new List<LogEvent>();

            //Open the JSON log file for the range of dates (and exclude machinename) Could be several for LB
            var dateRange = endDate - startDate;

            //Log Directory
            var logDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\";

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = $"*{day.ToString("yyyyMMdd")}.json";

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                //Foreach file we find - open it
                foreach(var filePath in filesForCurrentDay)
                {
                    //Open log file & add contents to the log collection
                    //Which we then use LINQ to page over
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
                }
            }
            
            return logs;
        }

        public int GetNumberOfErrors(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var logs = GetAllLogs(startDate, endDate);
            return logs.Count(x => x.Level == LogEventLevel.Fatal && x.Level == LogEventLevel.Error && x.Exception != null);
        }

        public LogLevelCounts GetLogLevelCounts(DateTimeOffset startDate, DateTimeOffset endDate)
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

        public IEnumerable<CommonLogMessage> GetCommonLogMessages(DateTimeOffset startDate, DateTimeOffset endDate, int numberOfResults)
        {
            var logs = GetAllLogs(startDate, endDate);
            var templates = logs.GroupBy(x => x.MessageTemplate.Text)
                .Select(g => new CommonLogMessage { MessageTemplate = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(numberOfResults);

            return templates;
        }

        public PagedResult<LogMessage> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate,
            int pageNumber = 1, int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null)
        {            
            //Get all logs into memory (Not sure this good or not)
            var logs = GetAllLogs(startDate, endDate);
            
            //If we have a filter expression, apply it
            if(string.IsNullOrEmpty(filterExpression) == false)
            {
                Func<LogEvent, bool> filter = null;
                var eval = FilterLanguage.CreateFilter(filterExpression);
                filter = evt => true.Equals(eval(evt));
                
                logs = logs.Where(filter);
            }

            long totalRecords = logs.Count();
            long pageIndex = pageNumber - 1;
            
            //Order By, Skip, Take & Select
            IEnumerable<LogMessage> logMessages = logs
                .OrderBy(l => l.Timestamp, orderDirection)
                .Skip(pageSize * (pageNumber -1))
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
                Items = logMessages
            };            
        }
    }
}
