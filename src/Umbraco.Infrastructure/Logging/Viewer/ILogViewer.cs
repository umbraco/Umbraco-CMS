using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Logging.Viewer;

public interface ILogViewer
{
    bool CanHandleLargeLogs { get; }

    /// <summary>
    ///     Get all saved searches from your chosen data source
    /// </summary>
    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> GetSavedSearches();

    /// <summary>
    ///     Adds a new saved search to chosen data source and returns the updated searches
    /// </summary>
    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

    /// <summary>
    ///     Deletes a saved search to chosen data source and returns the remaining searches
    /// </summary>
    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name);

    /// <summary>
    ///     A count of number of errors
    ///     By counting Warnings with Exceptions, Errors &amp; Fatal messages
    /// </summary>
    int GetNumberOfErrors(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns a number of the different log level entries
    /// </summary>
    [Obsolete("Use ILogViewerService.GetLogLevelCounts instead. Scheduled for removal in Umbraco 15.")]
    LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns a list of all unique message templates and their counts
    /// </summary>
    [Obsolete("Use ILogViewerService.GetMessageTemplates instead. Scheduled for removal in Umbraco 15.")]
    IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod);

    [Obsolete("Use ILogViewerService.CanViewLogsAsync instead. Scheduled for removal in Umbraco 15.")]
    bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

    /// <summary>
    ///     Returns the collection of logs
    /// </summary>
    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    PagedResult<LogMessage> GetLogs(
        LogTimePeriod logTimePeriod,
        int pageNumber = 1,
        int pageSize = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null);

    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    PagedModel<LogMessage> GetLogsAsPagedModel(
        LogTimePeriod logTimePeriod,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null);
}
