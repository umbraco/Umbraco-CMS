using System.Collections.Generic;
using Serilog.Events;

namespace Umbraco.Core.Logging.Viewer
{
    public interface ILogViewer
    {
        /// <summary>
        /// A count of number of errors
        /// By counting Warnings with Exceptions, Errors & Fatal messages
        /// </summary>
        int GetNumberOfErrors();

        /// <summary>
        /// Returns a number of the different log level entries
        /// </summary>
        LogLevelCounts GetLogLevelCounts();

        /// <summary>
        /// Returns the top 5 common log message templates and their counts
        /// </summary>
        IEnumerable<CommonLogMessage> GetCommonLogMessages();

        /// <summary>
        /// Returns the collection of logs
        /// </summary>
        IEnumerable<LogEvent> GetLogs();

    }
}
