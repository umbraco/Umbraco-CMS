using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Filters.Expressions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public abstract class LogViewerSourceBase : ILogViewer
    {
        private static readonly string expressionOperators = "()+=*<>%-";
        private static readonly string SearchesConfigPath = IOHelper.MapPath("~/Config/logviewer.searches.config.js");

        public abstract IEnumerable<LogEvent> GetAllLogs(DateTimeOffset startDate, DateTimeOffset endDate);

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
            var logs = GetAllLogs(startDate, endDate);
            return logs.Count(x => x.Level == LogEventLevel.Fatal || x.Level == LogEventLevel.Error || x.Exception != null);
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

        public IEnumerable<LogTemplate> GetMessageTemplates(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var logs = GetAllLogs(startDate, endDate);
            var templates = logs.GroupBy(x => x.MessageTemplate.Text)
                .Select(g => new LogTemplate { MessageTemplate = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            return templates;
        }

        public PagedResult<LogMessage> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate,
            int pageNumber = 1, int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null,
            string[] logLevels = null)
        {
            //Get all logs into memory (Not sure this good or not)
            var allLogs = GetAllLogs(startDate, endDate);
            var logs = allLogs;

            //If we have a filter expression check it and apply
            if (string.IsNullOrEmpty(filterExpression) == false)
            {
                Func<LogEvent, bool> filter = null;

                // If the expression is one word and doesn't contain a serilog operator then we can perform a like search
                if (!filterExpression.Contains(" ") && !filterExpression.ContainsAny(expressionOperators.Select(c => c)))
                {
                    filter = PerformMessageLikeFilter(filterExpression);
                }
                else // check if it's a valid expression
                {
                    // If the expression evaluates then make it into a filter
                    if (FilterLanguage.TryCreateFilter(filterExpression, out Func<LogEvent, object> eval, out string error))
                    {
                        filter = evt => true.Equals(eval(evt));
                    }
                    else
                    {
                        //Assume the expression was a search string and make a Like filter from that
                        filter = PerformMessageLikeFilter(filterExpression);
                    }
                }

                if (filter != null)
                {
                    logs = FilterLogs(logs, filter);
                }
            }

            //This is user used the checkbox UI to toggle which log levels they wish to see
            //If an empty array - its implied all levels to be viewed
            if (logLevels.Length > 0)
            {
                var logsAfterLevelFilters = new List<LogEvent>();
                foreach (var level in logLevels)
                {
                    logsAfterLevelFilters.AddRange(logs.Where(x => x.Level.ToString() == level));
                }
                logs = logsAfterLevelFilters;
            }

            long totalRecords = logs.Count();
            long pageIndex = pageNumber - 1;

            //Order By, Skip, Take & Select
            var logMessages = logs
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

        private Func<LogEvent, bool> PerformMessageLikeFilter(string filterExpression)
        {
            var filterSearch = $"@Message like '%{FilterLanguage.EscapeLikeExpressionContent(filterExpression)}%'";
            if (FilterLanguage.TryCreateFilter(filterSearch, out var eval, out var error))
            {
                return evt => true.Equals(eval(evt));
            }

            return null;
        }

        private IEnumerable<LogEvent> FilterLogs(IEnumerable<LogEvent> logs, Func<LogEvent, bool> filter)
        {
            if (filter != null)
            {
                logs = logs.Where(filter);
            }

            return logs;
        }
    }
}
