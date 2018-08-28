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
        IEnumerable<LogEvent> GetAllLogs(DateTime startDate, DateTime endDate);

        /// <summary>
        /// A count of number of errors
        /// By counting Warnings with Exceptions, Errors & Fatal messages
        /// </summary>
        int GetNumberOfErrors(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns a number of the different log level entries
        /// </summary>
        LogLevelCounts GetLogLevelCounts(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the top 10 common log message templates and their counts
        /// </summary>
        IEnumerable<CommonLogMessage> GetCommonLogMessages(DateTime startDate, DateTime endDate, int numberOfResults = 10);

        /// <summary>
        /// Returns the collection of logs
        /// </summary>
        PagedResult<LogMessage> GetLogs(DateTime startDate, DateTime endDate,
            int pageNumber = 1, int pageSize = 100, Direction orderDirection = Direction.Descending);

    }
}
