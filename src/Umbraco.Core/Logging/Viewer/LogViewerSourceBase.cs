using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public abstract class LogViewerSourceBase : ILogViewer
    {
        
        private static readonly string SearchesConfigPath = IOHelper.MapPath("~/Config/logviewer.searches.config.js");

        public abstract bool CanHandleLargeLogs { get; }
        
        public abstract IEnumerable<LogEvent> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate, ILogFilter filter, int skip, int take);
        
        public abstract bool CheckCanOpenLogs(DateTimeOffset startDate, DateTimeOffset endDate);

        public virtual IEnumerable<SavedLogSearch> GetSavedSearches()
        {
            //Our default implementation

            //If file does not exist - lets create it with an empty array
            IOHelper.EnsureFileExists(SearchesConfigPath, "[]");

            var rawJson = System.IO.File.ReadAllText(SearchesConfigPath);
            return JsonConvert.DeserializeObject<IEnumerable<SavedLogSearch>>(rawJson);
        }

        public virtual IEnumerable<SavedLogSearch> AddSavedSearch(string name, string query)
        {
            //Get the existing items
            var searches = GetSavedSearches().ToList();

            //Add the new item to the bottom of the list
            searches.Add(new SavedLogSearch { Name = name, Query = query });

            //Serilaize to JSON string
            var rawJson = JsonConvert.SerializeObject(searches, Formatting.Indented);

            //If file does not exist - lets create it with an empty array
            IOHelper.EnsureFileExists(SearchesConfigPath, "[]");

            //Write it back down to file
            System.IO.File.WriteAllText(SearchesConfigPath, rawJson);

            //Return the updated object - so we can instantly reset the entire array from the API response
            //As opposed to push a new item into the array
            return searches;
        }

        public virtual IEnumerable<SavedLogSearch> DeleteSavedSearch(string name, string query)
        {
            //Get the existing items
            var searches = GetSavedSearches().ToList();

            //Removes the search
            searches.RemoveAll(s => s.Name.Equals(name) && s.Query.Equals(query));

            //Serilaize to JSON string
            var rawJson = JsonConvert.SerializeObject(searches, Formatting.Indented);

            //Write it back down to file
            System.IO.File.WriteAllText(SearchesConfigPath, rawJson);

            //Return the updated object - so we can instantly reset the entire array from the API response
            return searches;
        }

        public int GetNumberOfErrors(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var errorCounter = new ErrorCounterFilter();
            GetLogs(startDate, endDate, errorCounter, 0, int.MaxValue);
            return errorCounter.count;
        }

        public LogLevelCounts GetLogLevelCounts(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var counter = new CountingFilter();
            GetLogs(startDate, endDate, counter, 0, int.MaxValue);
            return counter.Counts;
        }

        public IEnumerable<LogTemplate> GetMessageTemplates(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var messageTemplates = new MessageTemplateFilter();
            GetLogs(startDate, endDate, messageTemplates, 0, int.MaxValue);

            var templates = messageTemplates.counts.
                Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
                .OrderByDescending(x=> x.Count);

            return templates;
        }

        public PagedResult<LogMessage> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate,
            int pageNumber = 1, int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null,
            string[] logLevels = null)
        {
            var expression = new ExpressionFilter(filterExpression);
            var filteredLogs = GetLogs(startDate, endDate, expression, 0, int.MaxValue);
            
            //This is user used the checkbox UI to toggle which log levels they wish to see
            //If an empty array - its implied all levels to be viewed
            if (logLevels.Length > 0)
            {
                var logsAfterLevelFilters = new List<LogEvent>();
                foreach (var level in logLevels)
                {
                    logsAfterLevelFilters.AddRange(filteredLogs.Where(x => x.Level.ToString() == level));
                }
                filteredLogs = logsAfterLevelFilters;
            }

            long totalRecords = filteredLogs.Count();
            long pageIndex = pageNumber - 1;

            //Order By, Skip, Take & Select
            var logMessages = filteredLogs
                .OrderBy(l => l.Timestamp, orderDirection)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .Select(x => new LogMessage
                {
                    Timestamp = x.Timestamp,
                    Level = x.Level,
                    MessageTemplateText = x.MessageTemplate.Text,
                    Exception = x.Exception?.ToString(),
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
