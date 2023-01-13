using System.Collections.ObjectModel;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

// Add documentation
public interface ILogViewerService : IService
{
    /// <summary>
    ///     Gets all logs.
    /// </summary>
    Attempt<PagedModel<ILogEntry>> GetPagedLogs(
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null);

    #region Saved Log Search
    /// <summary>
    ///     Get all saved log queries from your chosen data source.
    /// </summary>
    Task<IReadOnlyList<ILogViewerQuery>> GetSavedLogQueriesAsync();

    /// <summary>
    ///     Gets a saved log query by name from your chosen data source.
    /// </summary>
    Task<ILogViewerQuery?> GetSavedLogQueryByNameAsync(string name);

    /// <summary>
    ///     Adds a new saved log query to your chosen data source.
    /// </summary>
    Task<bool> AddSavedLogQueryAsync(string name, string query);

    /// <summary>
    ///     Deletes a saved log query to your chosen data source.
    /// </summary>
    Task<bool> DeleteSavedLogQueryAsync(string name);

    #endregion

    /// <summary>
    ///     Returns a value indicating whether the log files for the given time
    ///     period are not too large to view (more than 1GB file).
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The value whether or not you are able to view the logs.</returns>
    Task<bool> CanViewLogsAsync(DateTime? startDate, DateTime? endDate);

    /// <summary>
    ///     Returns a number of the different log level entries.
    /// </summary>
    Attempt<LogLevelCounts> GetLogLevelCounts(DateTime? startDate, DateTime? endDate);

    /// <summary>
    ///     Returns a list of all unique message templates and their counts.
    /// </summary>
    Attempt<IEnumerable<LogTemplate>> GetMessageTemplates(DateTime? startDate, DateTime? endDate);

    /// <summary>
    ///     Get the log level values of the global minimum and the UmbracoFile one from the config file.
    /// </summary>
    ReadOnlyDictionary<string, LogLevel> GetLogLevelsFromSinks();

    /// <summary>
    ///     Get the minimum log level value from the config file.
    /// </summary>
    LogLevel GetGlobalMinLogLevel();
}
