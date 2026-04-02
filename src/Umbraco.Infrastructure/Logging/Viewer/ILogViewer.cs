using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents a log viewer that provides access to log entries.
/// </summary>
[Obsolete("Use ILogViewerService instead. Scheduled for removal in Umbraco 18.")]
public interface ILogViewer
{
    /// <summary>
    /// Gets a value indicating whether this log viewer supports processing large log files efficiently.
    /// </summary>
    bool CanHandleLargeLogs { get; }

    /// <summary>
    ///     Retrieves all saved log searches from the configured data source.
    /// </summary>
    /// <returns>A read-only list of saved log searches.</returns>
    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> GetSavedSearches();

    /// <summary>
    ///     Adds a new saved search to chosen data source and returns the updated searches
    /// </summary>
    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

    /// <summary>
    ///     Deletes a saved search from the chosen data source and returns the remaining saved searches.
    /// </summary>
    /// <param name="name">The name of the saved search to delete.</param>
    /// <returns>A read-only list of the remaining saved log searches after deletion.</returns>
    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name);

    /// <summary>
    ///     Returns the number of error log entries within a specified time period.
    ///     Errors are counted by including warnings with exceptions, as well as error and fatal messages.
    /// </summary>
    /// <param name="logTimePeriod">The time period over which to count errors.</param>
    /// <returns>The number of errors in the specified time period.</returns>
    int GetNumberOfErrors(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns the counts of log entries for each log level within the specified time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period over which to count log levels.</param>
    /// <returns>A <see cref="LogLevelCounts"/> object representing the counts of each log level.</returns>
    [Obsolete("Use ILogViewerService.GetLogLevelCounts instead. Scheduled for removal in Umbraco 15.")]
    LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns a list of all unique message templates and their counts
    /// </summary>
    [Obsolete("Use ILogViewerService.GetMessageTemplates instead. Scheduled for removal in Umbraco 15.")]
    IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod);

    /// <summary>
    /// Determines whether the logs can be opened for the specified time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period for which to check log accessibility.</param>
    /// <returns>True if logs can be opened for the specified time period; otherwise, false.</returns>
    [Obsolete("Use ILogViewerService.CanViewLogsAsync instead. Scheduled for removal in Umbraco 15.")]
    bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns a paged collection of log messages for the specified time period and filter criteria.
    /// </summary>
    /// <param name="logTimePeriod">The time period for which to retrieve logs.</param>
    /// <param name="pageNumber">The page number to retrieve. Defaults to 1.</param>
    /// <param name="pageSize">The number of log entries per page. Defaults to 100.</param>
    /// <param name="orderDirection">The direction in which to order the logs (ascending or descending). Defaults to descending.</param>
    /// <param name="filterExpression">An optional filter expression to filter logs.</param>
    /// <param name="logLevels">An optional array of log levels to include.</param>
    /// <returns>A paged result containing the log messages.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.GetPagedLogs"/> instead. Scheduled for removal in Umbraco 15.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    PagedResult<LogMessage> GetLogs(
        LogTimePeriod logTimePeriod,
        int pageNumber = 1,
        int pageSize = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null);

    /// <summary>
    /// Retrieves a paged model of log messages filtered by the specified parameters.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter logs.</param>
    /// <param name="skip">The number of log entries to skip.</param>
    /// <param name="take">The number of log entries to take.</param>
    /// <param name="orderDirection">The direction to order the log entries. Defaults to <see cref="Direction.Descending"/>.</param>
    /// <param name="filterExpression">An optional filter expression to apply to the logs.</param>
    /// <param name="logLevels">An optional array of log levels to filter by.</param>
    /// <returns>A paged model containing the filtered log messages.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ILogViewerService.GetPagedLogs</c> instead.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    PagedModel<LogMessage> GetLogsAsPagedModel(
        LogTimePeriod logTimePeriod,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null);
}
