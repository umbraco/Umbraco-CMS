using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Provides a base class for log viewer repository implementations.
/// </summary>
public abstract class LogViewerRepositoryBase : ILogViewerRepository
{
    private readonly UmbracoFileConfiguration _umbracoFileConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerRepositoryBase"/> class.
    /// </summary>
    /// <param name="umbracoFileConfig"></param>
    public LogViewerRepositoryBase(UmbracoFileConfiguration umbracoFileConfig) => _umbracoFileConfig = umbracoFileConfig;

    /// <inheritdoc />
    public virtual IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, string? filterExpression = null)
    {
        var expressionFilter = new ExpressionFilter(filterExpression);

        return GetLogs(logTimePeriod, expressionFilter);
    }

    /// <inheritdoc />
    public virtual LogLevelCounts GetLogCount(LogTimePeriod logTimePeriod)
    {
        var counter = new CountingFilter();

        GetLogs(logTimePeriod, counter);

        return counter.Counts;
    }

    /// <inheritdoc />
    public virtual LogTemplate[] GetMessageTemplates(LogTimePeriod logTimePeriod)
    {
        var messageTemplates = new MessageTemplateFilter();

        GetLogs(logTimePeriod, messageTemplates);

        return messageTemplates.Counts
            .Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
            .OrderByDescending(x => x.Count).ToArray();
    }

    /// <inheritdoc />
    public virtual LogLevel GetGlobalMinLogLevel()
    {
        LogEventLevel logLevel = GetGlobalLogLevelEventMinLevel();

        return Enum.Parse<LogLevel>(logLevel.ToString());
    }

    /// <summary>
    /// Gets the minimum-level log value from the config file.
    /// </summary>
    public virtual LogLevel RestrictedToMinimumLevel()
    {
        LogEventLevel minLevel = _umbracoFileConfig.RestrictedToMinimumLevel;
        return Enum.Parse<LogLevel>(minLevel.ToString());
    }

    /// <summary>
    /// Gets the minimum log level from the global Serilog configuration.
    /// </summary>
    protected virtual LogEventLevel GetGlobalLogLevelEventMinLevel() =>
        Enum.GetValues(typeof(LogEventLevel))
            .Cast<LogEventLevel>()
            .Where(Log.IsEnabled)
            .DefaultIfEmpty(LogEventLevel.Information)
            .Min();

    /// <summary>
    /// Retrieves the logs for a specified time period and filter.
    /// </summary>
    protected abstract IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, ILogFilter logFilter);
}
