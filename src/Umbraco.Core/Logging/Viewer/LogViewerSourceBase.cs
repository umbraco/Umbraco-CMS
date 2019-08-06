﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Formatting = Newtonsoft.Json.Formatting;

namespace Umbraco.Core.Logging.Viewer
{
    public abstract class LogViewerSourceBase : ILogViewer
    {
        protected LogViewerSourceBase(string pathToSearches = "")
        {
            if (string.IsNullOrEmpty(pathToSearches))
                // ReSharper disable once StringLiteralTypo
                pathToSearches = IOHelper.MapPath("~/Config/logviewer.searches.config.js");

            _searchesConfigPath = pathToSearches;
        }

        private readonly string _searchesConfigPath;

        public abstract bool CanHandleLargeLogs { get; }

        /// <summary>
        /// Get all logs from your chosen data source back as Serilog LogEvents
        /// </summary>
        protected abstract IReadOnlyList<LogEvent> GetLogs(LogTimePeriod logTimePeriod, ILogFilter filter, int skip, int take);

        public abstract bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

        public virtual IReadOnlyList<SavedLogSearch> GetSavedSearches()
        {
            //Our default implementation

            //If file does not exist - lets create it with an empty array
            EnsureFileExists(_searchesConfigPath, "[]");

            var rawJson = System.IO.File.ReadAllText(_searchesConfigPath);
            return JsonConvert.DeserializeObject<SavedLogSearch[]>(rawJson);
        }

        public virtual IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
        {
            //Get the existing items
            var searches = GetSavedSearches().ToList();

            //Add the new item to the bottom of the list
            searches.Add(new SavedLogSearch { Name = name, Query = query });

            //Serialize to JSON string
            var rawJson = JsonConvert.SerializeObject(searches, Formatting.Indented);

            //If file does not exist - lets create it with an empty array
            EnsureFileExists(_searchesConfigPath, "[]");

            //Write it back down to file
            System.IO.File.WriteAllText(_searchesConfigPath, rawJson);

            //Return the updated object - so we can instantly reset the entire array from the API response
            //As opposed to push a new item into the array
            return searches;
        }

        public virtual IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name, string query)
        {
            //Get the existing items
            var searches = GetSavedSearches().ToList();

            //Removes the search
            searches.RemoveAll(s => s.Name.Equals(name) && s.Query.Equals(query));

            //Serialize to JSON string
            var rawJson = JsonConvert.SerializeObject(searches, Formatting.Indented);

            //Write it back down to file
            System.IO.File.WriteAllText(_searchesConfigPath, rawJson);

            //Return the updated object - so we can instantly reset the entire array from the API response
            return searches;
        }

        public int GetNumberOfErrors(LogTimePeriod logTimePeriod)
        {
            var errorCounter = new ErrorCounterFilter();
            GetLogs(logTimePeriod, errorCounter, 0, int.MaxValue);
            return errorCounter.Count;
        }

        /// <summary>
        /// Get the Serilog minimum-level value from the config file. 
        /// </summary>
        /// <returns></returns>
        public string GetLogLevel()
        {
            var logLevel = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>().Where(Log.Logger.IsEnabled)?.Min() ?? null;
            return logLevel?.ToString() ?? "";
        }

        public LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod)
        {
            var counter = new CountingFilter();
            GetLogs(logTimePeriod, counter, 0, int.MaxValue);
            return counter.Counts;
        }

        public IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod)
        {
            var messageTemplates = new MessageTemplateFilter();
            GetLogs(logTimePeriod, messageTemplates, 0, int.MaxValue);

            var templates = messageTemplates.Counts.
                Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
                .OrderByDescending(x=> x.Count);

            return templates;
        }

        public PagedResult<LogMessage> GetLogs(LogTimePeriod logTimePeriod,
            int pageNumber = 1, int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null,
            string[] logLevels = null)
        {
            var expression = new ExpressionFilter(filterExpression);
            var filteredLogs = GetLogs(logTimePeriod, expression, 0, int.MaxValue);

            //This is user used the checkbox UI to toggle which log levels they wish to see
            //If an empty array or null - its implied all levels to be viewed
            if (logLevels?.Length > 0)
            {
                var logsAfterLevelFilters = new List<LogEvent>();
                var validLogType = true;
                foreach (var level in logLevels)
                {
                    //Check if level string is part of the LogEventLevel enum
                    if(Enum.IsDefined(typeof(LogEventLevel), level))
                    {
                        validLogType = true;
                        logsAfterLevelFilters.AddRange(filteredLogs.Where(x => string.Equals(x.Level.ToString(), level, StringComparison.InvariantCultureIgnoreCase)));
                    }
                    else
                    {
                        validLogType = false;
                    }
                }

                if (validLogType)
                {
                    filteredLogs = logsAfterLevelFilters;
                }
            }

            long totalRecords = filteredLogs.Count;

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

        private static void EnsureFileExists(string path, string contents)
        {
            var absolutePath = IOHelper.MapPath(path);
            if (System.IO.File.Exists(absolutePath)) return;

            using (var writer = System.IO.File.CreateText(absolutePath))
            {
                writer.Write(contents);
            }
        }
    }
}
