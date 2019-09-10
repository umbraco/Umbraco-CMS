﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public interface ILogViewer
    {
        /// <summary>
        /// Get all saved searches from your chosen data source
        /// </summary>
        IReadOnlyList<SavedLogSearch> GetSavedSearches();

        /// <summary>
        /// Adds a new saved search to chosen data source and returns the updated searches
        /// </summary>
        IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

        /// <summary>
        /// Deletes a saved search to chosen data source and returns the remaining searches
        /// </summary>
        IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name, string query);

        /// <summary>
        /// A count of number of errors
        /// By counting Warnings with Exceptions, Errors & Fatal messages
        /// </summary>
        int GetNumberOfErrors(LogTimePeriod logTimePeriod);

        /// <summary>
        /// Returns a number of the different log level entries
        /// </summary>
        LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod);

        /// <summary>
        /// Returns a list of all unique message templates and their counts
        /// </summary>
        IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod);

        bool CanHandleLargeLogs { get; }

        bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

        /// <summary>
        /// Gets the current Serilog minimum log level
        /// </summary>
        /// <returns></returns>
        string GetLogLevel();

        /// <summary>
        /// Returns the collection of logs
        /// </summary>
        PagedResult<LogMessage> GetLogs(LogTimePeriod logTimePeriod,
            int pageNumber = 1,
            int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null,
            string[] logLevels = null);

    }
}
