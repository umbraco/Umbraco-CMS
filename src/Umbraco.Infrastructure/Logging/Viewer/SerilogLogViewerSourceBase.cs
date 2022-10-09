using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Viewer;

public abstract class SerilogLogViewerSourceBase : ILogViewer
{
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly ILogViewerConfig _logViewerConfig;
    private readonly ILogger _serilogLog;

    [Obsolete("Please use ctor with all params instead. Scheduled for removal in V11.")]
    protected SerilogLogViewerSourceBase(ILogViewerConfig logViewerConfig, ILogger serilogLog)
    {
        _logViewerConfig = logViewerConfig;
        _logLevelLoader = StaticServiceProvider.Instance.GetRequiredService<ILogLevelLoader>();
        _serilogLog = serilogLog;
    }

    protected SerilogLogViewerSourceBase(ILogViewerConfig logViewerConfig, ILogLevelLoader logLevelLoader, ILogger serilogLog)
    {
        _logViewerConfig = logViewerConfig;
        _logLevelLoader = logLevelLoader;
        _serilogLog = serilogLog;
    }

    public abstract bool CanHandleLargeLogs { get; }

    public abstract bool CheckCanOpenLogs(LogTimePeriod logTimePeriod);

    public virtual IReadOnlyList<SavedLogSearch>? GetSavedSearches()
        => _logViewerConfig.GetSavedSearches();

    public virtual IReadOnlyList<SavedLogSearch>? AddSavedSearch(string? name, string? query)
        => _logViewerConfig.AddSavedSearch(name, query);

    public virtual IReadOnlyList<SavedLogSearch>? DeleteSavedSearch(string? name, string? query)
        => _logViewerConfig.DeleteSavedSearch(name, query);

    public int GetNumberOfErrors(LogTimePeriod logTimePeriod)
    {
        var errorCounter = new ErrorCounterFilter();
        GetLogs(logTimePeriod, errorCounter, 0, int.MaxValue);
        return errorCounter.Count;
    }

    /// <summary>
    ///     Get the Serilog minimum-level value from the config file.
    /// </summary>
    [Obsolete("Please use LogLevelLoader.GetGlobalMinLogLevel() instead. Scheduled for removal in V11.")]
    public string GetLogLevel()
    {
        LogEventLevel? logLevel = Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>()
            .Where(_serilogLog.IsEnabled).DefaultIfEmpty(LogEventLevel.Information).Min();
        return logLevel?.ToString() ?? string.Empty;
    }

    public LogLevelCounts GetLogLevelCounts(LogTimePeriod logTimePeriod)
    {
        var counter = new CountingFilter();
        GetLogs(logTimePeriod, counter, 0, int.MaxValue);
        return counter.Counts;
    }

    public IEnumerable<LogTemplate> GetMessageTemplates(LogTimePeriod logTimePeriod)
    {
        var messageTemplates = new MessageTemplateFilter();
        GetLogs(logTimePeriod, messageTemplates, 0, int.MaxValue);

        IOrderedEnumerable<LogTemplate> templates = messageTemplates.Counts
            .Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
            .OrderByDescending(x => x.Count);

        return templates;
    }

    public PagedResult<LogMessage> GetLogs(
        LogTimePeriod logTimePeriod,
        int pageNumber = 1,
        int pageSize = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null)
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
    ///     Get the Serilog minimum-level and UmbracoFile-level values from the config file.
    /// </summary>
    public ReadOnlyDictionary<string, LogEventLevel?> GetLogLevels() => _logLevelLoader.GetLogLevelsFromSinks();

    /// <summary>
    ///     Get all logs from your chosen data source back as Serilog LogEvents
    /// </summary>
    protected abstract IReadOnlyList<LogEvent> GetLogs(LogTimePeriod logTimePeriod, ILogFilter filter, int skip, int take);
}
