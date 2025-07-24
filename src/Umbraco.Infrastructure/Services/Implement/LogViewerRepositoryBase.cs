using Serilog.Events;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public abstract class LogViewerRepositoryBase : ILogViewerRepository
{
    private readonly UmbracoFileConfiguration _umbracoFileConfig;

    public LogViewerRepositoryBase(UmbracoFileConfiguration umbracoFileConfig)
    {
        _umbracoFileConfig = umbracoFileConfig;
    }

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

    public virtual LogLevel RestrictedToMinimumLevel()
    {
        LogEventLevel minLevel = _umbracoFileConfig.RestrictedToMinimumLevel;
        return Enum.Parse<LogLevel>(minLevel.ToString());
    }

    protected abstract LogEventLevel GetGlobalLogLevelEventMinLevel();

    protected abstract IEnumerable<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, ILogFilter logFilter);
}
