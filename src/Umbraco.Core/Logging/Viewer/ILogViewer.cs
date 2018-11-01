using Serilog.Events;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Logging.Viewer
{
    public interface ILogViewer
    {
        /// <summary>
        /// Get all logs from your chosen datasource back as Serilog LogEvents
        /// </summary>
        IEnumerable<LogEvent> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate, ILogFilter filter, int skip, int take);

        /// <summary>
        /// Get all saved searches from your chosen datasource
        /// </summary>
        IEnumerable<SavedLogSearch> GetSavedSearches();

        /// <summary>
        /// Adds a new saved search to chosen datasource and returns the updated searches
        /// </summary>
        IEnumerable<SavedLogSearch> AddSavedSearch(string name, string query);

        /// <summary>
        /// Deletes a saved search to chosen datasource and returns the remaining searches
        /// </summary>
        IEnumerable<SavedLogSearch> DeleteSavedSearch(string name, string query);

        /// <summary>
        /// A count of number of errors
        /// By counting Warnings with Exceptions, Errors & Fatal messages
        /// </summary>
        int GetNumberOfErrors(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Returns a number of the different log level entries
        /// </summary>
        LogLevelCounts GetLogLevelCounts(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Returns a list of all unique message templates and their counts
        /// </summary>
        IEnumerable<LogTemplate> GetMessageTemplates(DateTimeOffset startDate, DateTimeOffset endDate);

        long GetLogSize(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Returns the collection of logs
        /// </summary>
        PagedResult<LogMessage> GetLogs(DateTimeOffset startDate, DateTimeOffset endDate,
            int pageNumber = 1,
            int pageSize = 100,
            Direction orderDirection = Direction.Descending,
            string filterExpression = null,
            string[] logLevels = null);

    }
}
