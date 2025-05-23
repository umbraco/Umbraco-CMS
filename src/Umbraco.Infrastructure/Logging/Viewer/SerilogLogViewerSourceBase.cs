using System.Collections.ObjectModel;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Viewer;

public abstract class SerilogLogViewerSourceBase : ILogViewer
{
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly ILogViewerConfig _logViewerConfig;

    protected SerilogLogViewerSourceBase(ILogViewerConfig logViewerConfig, ILogLevelLoader logLevelLoader, ILogger serilogLog)
    {
        _logViewerConfig = logViewerConfig;
        _logLevelLoader = logLevelLoader;
    }

    public abstract bool CanHandleLargeLogs { get; }

    [Obsolete("Use ILogViewerService.CanViewLogsAsync instead. Scheduled for removal in Umbraco 15.")]
    public abstract bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> GetSavedSearches()
        => _logViewerConfig.GetSavedSearches();

    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
        => _logViewerConfig.AddSavedSearch(name, query);

    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public virtual IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name)
        => _logViewerConfig.DeleteSavedSearch(name);

    public int GetNumberOfErrors(LogTimePeriod logTimePeriod)
    {
        var errorCounter = new ErrorCounterFilter();
        GetLogs(logTimePeriod, errorCounter, 0, int.MaxValue);
        return errorCounter.Count;
    }

    [Obsolete("Use ILogViewerService.GetLogLevelCounts instead. Scheduled for removal in Umbraco 15.")]
    public LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod)
    {
        var counter = new CountingFilter();
        GetLogs(logTimePeriod, counter, 0, int.MaxValue);
        return counter.Counts;
    }

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
    ///     Get the Serilog minimum-level and UmbracoFile-level values from the config file.
    /// </summary>
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
