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
    ///     Gets all logs as a paged model. The attempt will fail if the log files
    ///     for the given time period are too large (more than 1GB).
    /// </summary>
    /// <param name="startDate">The start date for the date range.</param>
    /// <param name="endDate">The end date for the date range.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="orderDirection">
    ///     The direction in which the log entries are to be ordered.
    /// </param>
    /// <param name="filterExpression">The query expression to filter on.</param>
    /// <param name="logLevels">The log levels for which to retrieve the log messages.</param>
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
    /// <param name="name">The name of the saved log query.</param>
    Task<ILogViewerQuery?> GetSavedLogQueryByNameAsync(string name);

    /// <summary>
    ///     Adds a new saved log query to your chosen data source.
    /// </summary>
    /// <param name="name">The name of the new saved log query.</param>
    /// <param name="query">The query of the new saved log query.</param>
    Task<bool> AddSavedLogQueryAsync(string name, string query);

    /// <summary>
    ///     Deletes a saved log query to your chosen data source.
    /// </summary>
    /// <param name="name">The name of the saved log search.</param>
    Task<bool> DeleteSavedLogQueryAsync(string name);

    #endregion

    /// <summary>
    ///     Returns a value indicating whether the log files for the given time
    ///     period are not too large to view (more than 1GB).
    /// </summary>
    /// <param name="startDate">The start date for the date range.</param>
    /// <param name="endDate">The end date for the date range.</param>
    /// <returns>The value whether or not you are able to view the logs.</returns>
    Task<bool> CanViewLogsAsync(DateTime? startDate, DateTime? endDate);

    /// <summary>
    ///     Returns a number of the different log level entries.
    ///     The attempt will fail if the log files for the given
    ///     time period are too large (more than 1GB).
    /// </summary>
    /// <param name="startDate">The start date for the date range.</param>
    /// <param name="endDate">The end date for the date range.</param>
    Attempt<LogLevelCounts> GetLogLevelCounts(DateTime? startDate, DateTime? endDate);

    /// <summary>
    ///     Returns a list of all unique message templates and their counts.
    ///     The attempt will fail if the log files for the given
    ///     time period are too large (more than 1GB).
    /// </summary>
    /// <param name="startDate">The start date for the date range.</param>
    /// <param name="endDate">The end date for the date range.</param>
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
