using System.Collections.ObjectModel;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Serves as the base class for log viewer sources that utilize Serilog, providing shared functionality for derived log viewer implementations.
/// </summary>
[Obsolete("Use ILogViewerService instead. Scheduled for removal in Umbraco 18.")]
public abstract class SerilogLogViewerSourceBase : ILogViewer
{
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly ILogViewerConfig _logViewerConfig;

    protected SerilogLogViewerSourceBase(ILogViewerConfig logViewerConfig, ILogLevelLoader logLevelLoader, ILogger serilogLog)
    {
        _logViewerConfig = logViewerConfig;
        _logLevelLoader = logLevelLoader;
    }

    /// <summary>
    /// Gets a value indicating whether this log viewer source is capable of efficiently handling large log files.
    /// </summary>
    public abstract bool CanHandleLargeLogs { get; }

    /// <summary>
    /// Determines whether logs can be opened for the specified time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period for which to check log availability.</param>
    /// <returns>True if logs can be opened for the specified time period; otherwise, false.</returns>
    [Obsolete("Use ILogViewerService.CanViewLogsAsync instead. Scheduled for removal in Umbraco 15.")]
    public abstract bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

    /// <summary>
    /// Gets the saved log searches.
    /// </summary>
    /// <returns>A read-only list of saved log searches.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.GetSavedLogQueriesAsync"/> instead.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> GetSavedSearches()
        => _logViewerConfig.GetSavedSearches();

    /// <summary>
    /// Adds a saved log search with the specified name and query.
    /// </summary>
    /// <param name="name">The name of the saved search.</param>
    /// <param name="query">The query string for the saved search.</param>
    /// <returns>A read-only list of saved log searches including the newly added one.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.AddSavedLogQueryAsync"/> instead.
    /// </remarks>
    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
        => _logViewerConfig.AddSavedSearch(name, query);

    /// <summary>
    /// Deletes a saved log search by name and returns the updated list of saved log searches.
    /// </summary>
    /// <param name="name">The name of the saved log search to delete.</param>
    /// <returns>The updated list of saved log searches after deletion.</returns>
    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name)
        => _logViewerConfig.DeleteSavedSearch(name);

    /// <summary>
    /// Gets the number of errors within the specified log time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period for which to count errors.</param>
    /// <returns>The count of errors in the specified time period.</returns>
    public int GetNumberOfErrors(LogTimePeriod logTimePeriod)
    {
        var errorCounter = new ErrorCounterFilter();
        GetLogs(logTimePeriod, errorCounter, 0, int.MaxValue);
        return errorCounter.Count;
    }

    /// <summary>
    /// Returns the number of log entries for each log level within the specified time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period to analyze for log level counts.</param>
    /// <returns>A <see cref="Umbraco.Cms.Core.Logging.Viewer.LogLevelCounts"/> object containing the count of log entries per log level.</returns>
    [Obsolete("Use ILogViewerService.GetLogLevelCounts instead. Scheduled for removal in Umbraco 15.")]
    public LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod)
    {
        var counter = new CountingFilter();
        GetLogs(logTimePeriod, counter, 0, int.MaxValue);
        return counter.Counts;
    }

    /// <summary>
    /// Retrieves a collection of log message templates and their occurrence counts within the specified time period.
    /// </summary>
    /// <param name="logTimePeriod">The time period used to filter log entries.</param>
    /// <returns>An ordered enumerable of <see cref="LogTemplate"/> objects, each representing a unique message template and its count.</returns>
    [Obsolete("Use ILogViewerService.GetMessageTemplates instead. Scheduled for removal in Umbraco 15.")]
    public IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod)
    {
        var messageTemplates = new MessageTemplateFilter();
        GetLogs(logTimePeriod, messageTemplates, 0, int.MaxValue);

        IOrderedEnumerable<LogTemplate> templates = messageTemplates.Counts
            .Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
            .OrderByDescending(x => x.Count);

        return templates;
    }

    /// <summary>
    /// Retrieves a paged list of log messages filtered by the specified time period, filter expression, and log levels.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter logs.</param>
    /// <param name="pageNumber">The page number to retrieve. The default is 1.</param>
    /// <param name="pageSize">The number of log messages per page. The default is 100.</param>
    /// <param name="orderDirection">The direction in which to order the logs by timestamp. The default is <see cref="Direction.Descending"/>.</param>
    /// <param name="filterExpression">An optional filter expression to filter log messages.</param>
    /// <param name="logLevels">An optional array of log levels to include.</param>
    /// <returns>A <see cref="PagedResult{LogMessage}"/> containing the filtered log messages.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ILogViewerService.GetPagedLogs</c> instead. Scheduled for removal in Umbraco 15.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    public PagedResult<LogMessage> GetLogs(
        LogTimePeriod logTimePeriod,
        int pageNumber = 1,
        int pageSize = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null)
    {
        IReadOnlyList<LogEvent> filteredLogs = GetFilteredLogs(logTimePeriod, filterExpression, logLevels);
        long totalRecords = filteredLogs.Count;

        // Order By, Skip, Take & Select
        IEnumerable<LogMessage> logMessages = filteredLogs
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
                RenderedMessage = x.RenderMessage(),
            });

        return new PagedResult<LogMessage>(totalRecords, pageNumber, pageSize) { Items = logMessages };
    }

    /// <summary>
    /// Retrieves log messages as a paged model, filtered and ordered according to the specified parameters.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter logs by.</param>
    /// <param name="skip">The number of log entries to skip (for pagination).</param>
    /// <param name="take">The number of log entries to return (for pagination).</param>
    /// <param name="orderDirection">The direction to order the logs by timestamp (ascending or descending). Defaults to <see cref="Direction.Descending"/>.</param>
    /// <param name="filterExpression">An optional filter expression to further filter logs.</param>
    /// <param name="logLevels">An optional array of log levels to filter logs.</param>
    /// <returns>A <see cref="PagedModel{LogMessage}"/> containing the filtered and paged log messages.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ILogViewerService.GetPagedLogs</c> instead. Scheduled for removal in Umbraco 15.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetPagedLogs instead. Scheduled for removal in Umbraco 15.")]
    public PagedModel<LogMessage> GetLogsAsPagedModel(
        LogTimePeriod logTimePeriod,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null)
    {
        IReadOnlyList<LogEvent> filteredLogs = GetFilteredLogs(logTimePeriod, filterExpression, logLevels);

        // Order By, Skip, Take & Select
        IEnumerable<LogMessage> logMessages = filteredLogs
            .OrderBy(l => l.Timestamp, orderDirection)
            .Select(x => new LogMessage
            {
                Timestamp = x.Timestamp,
                Level = x.Level,
                MessageTemplateText = x.MessageTemplate.Text,
                Exception = x.Exception?.ToString(),
                Properties = x.Properties,
                RenderedMessage = x.RenderMessage(),
            });

        return new PagedModel<LogMessage>(logMessages.Count(), logMessages.Skip(skip).Take(take));
    }

    /// <summary>
    /// Retrieves the Serilog minimum-level and UmbracoFile-level values from the configuration file.
    /// </summary>
    /// <returns>
    /// A read-only dictionary that maps sink names to their corresponding <see cref="LogEventLevel"/> values, or <c>null</c> if not set.
    /// </returns>
    [Obsolete("Use ILogViewerService.GetLogLevelsFromSinks instead. Scheduled for removal in Umbraco 15.")]
    public ReadOnlyDictionary<string, LogEventLevel?> GetLogLevels() => _logLevelLoader.GetLogLevelsFromSinks();

    /// <summary>
    ///     Get all logs from your chosen data source back as Serilog LogEvents
    /// </summary>
    protected abstract IReadOnlyList<LogEvent> GetLogs(LogTimePeriod logTimePeriod, ILogFilter filter, int skip, int take);

    private IReadOnlyList<LogEvent> GetFilteredLogs(
        LogTimePeriod logTimePeriod,
        string? filterExpression,
        string[]? logLevels)
    {
        var expression = new ExpressionFilter(filterExpression);
        IReadOnlyList<LogEvent> filteredLogs = GetLogs(logTimePeriod, expression, 0, int.MaxValue);

        // This is user used the checkbox UI to toggle which log levels they wish to see
        // If an empty array or null - its implied all levels to be viewed
        if (logLevels?.Length > 0)
        {
            var logsAfterLevelFilters = new List<LogEvent>();
            var validLogType = true;
            foreach (var level in logLevels)
            {
                // Check if level string is part of the LogEventLevel enum
                if (Enum.IsDefined(typeof(LogEventLevel), level))
                {
                    validLogType = true;
                    logsAfterLevelFilters.AddRange(filteredLogs.Where(x =>
                        string.Equals(x.Level.ToString(), level, StringComparison.InvariantCultureIgnoreCase)));
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

        return filteredLogs;
    }
}
