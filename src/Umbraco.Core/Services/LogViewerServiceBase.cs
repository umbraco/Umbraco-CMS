using System.Collections.ObjectModel;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for log viewer services that provides common functionality for managing log entries and queries.
/// </summary>
public abstract class LogViewerServiceBase : ILogViewerService
{
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly ICoreScopeProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerServiceBase"/> class.
    /// </summary>
    protected LogViewerServiceBase(
        ILogViewerQueryRepository logViewerQueryRepository,
        ICoreScopeProvider provider,
        ILogViewerRepository logViewerRepository)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _provider = provider;
        LogViewerRepository = logViewerRepository;
    }

    /// <summary>
    /// Gets the <see cref="ILogViewerRepository"/>.
    /// </summary>
    protected ILogViewerRepository LogViewerRepository { get; }

    /// <summary>
    /// Gets the name of the logger.
    /// </summary>
    protected abstract string LoggerName { get; }

    /// <inheritdoc/>
    public virtual ReadOnlyDictionary<string, LogLevel> GetLogLevelsFromSinks()
    {
        var configuredLogLevels = new Dictionary<string, LogLevel>
        {
            { "Global", GetGlobalMinLogLevel() },
            { LoggerName, LogViewerRepository.RestrictedToMinimumLevel() },
        };

        return configuredLogLevels.AsReadOnly();
    }

    /// <inheritdoc/>
    public virtual LogLevel GetGlobalMinLogLevel() => LogViewerRepository.GetGlobalMinLogLevel();

    /// <inheritdoc/>
    public virtual Task<ILogViewerQuery?> GetSavedLogQueryByNameAsync(string name)
    {
        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_logViewerQueryRepository.GetByName(name));
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<ILogViewerQuery?, LogViewerOperationStatus>> AddSavedLogQueryAsync(string name, string query)
    {
        ILogViewerQuery? logViewerQuery = await GetSavedLogQueryByNameAsync(name);

        if (logViewerQuery is not null)
        {
            return Attempt.FailWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(
                LogViewerOperationStatus.DuplicateLogSearch, null);
        }

        logViewerQuery = new LogViewerQuery(name, query);

        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        _logViewerQueryRepository.Save(logViewerQuery);

        return Attempt.SucceedWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            logViewerQuery);
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<ILogViewerQuery?, LogViewerOperationStatus>> DeleteSavedLogQueryAsync(string name)
    {
        ILogViewerQuery? logViewerQuery = await GetSavedLogQueryByNameAsync(name);

        if (logViewerQuery is null)
        {
            return Attempt.FailWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(
                LogViewerOperationStatus.NotFoundLogSearch, null);
        }

        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        _logViewerQueryRepository.Delete(logViewerQuery);

        return Attempt.SucceedWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            logViewerQuery);
    }

    /// <inheritdoc/>
    public virtual Task<PagedModel<ILogViewerQuery>> GetSavedLogQueriesAsync(int skip, int take)
    {
        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        ILogViewerQuery[] savedLogQueries = _logViewerQueryRepository.GetMany().ToArray();
        var pagedModel = new PagedModel<ILogViewerQuery>(savedLogQueries.Length, savedLogQueries.Skip(skip).Take(take));
        return Task.FromResult(pagedModel);
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<LogLevelCounts?, LogViewerOperationStatus>> GetLogLevelCountsAsync(
        DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        Attempt<bool, LogViewerOperationStatus> canViewLogs = await CanViewLogsAsync(logTimePeriod);

        // We will need to stop the request if trying to do this on a 1GB file
        if (canViewLogs.Success == false)
        {
            return Attempt.FailWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null);
        }

        LogLevelCounts counter = LogViewerRepository.GetLogCount(logTimePeriod);

        return Attempt.SucceedWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            counter);
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<PagedModel<LogTemplate>, LogViewerOperationStatus>> GetMessageTemplatesAsync(
        DateTimeOffset? startDate, DateTimeOffset? endDate, int skip, int take)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        Attempt<bool, LogViewerOperationStatus> canViewLogs = await CanViewLogsAsync(logTimePeriod);

        // We will need to stop the request if trying to do this on a 1GB file
        if (canViewLogs.Success == false)
        {
            return Attempt.FailWithStatus<PagedModel<LogTemplate>, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null!);
        }

        LogTemplate[] messageTemplates = LogViewerRepository.GetMessageTemplates(logTimePeriod);

        return Attempt.SucceedWithStatus(
            LogViewerOperationStatus.Success,
            new PagedModel<LogTemplate>(messageTemplates.Length, messageTemplates.Skip(skip).Take(take)));
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<PagedModel<ILogEntry>?, LogViewerOperationStatus>> GetPagedLogsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        Attempt<bool, LogViewerOperationStatus> canViewLogs = await CanViewLogsAsync(logTimePeriod);

        // We will need to stop the request if trying to do this on a 1GB file
        if (canViewLogs.Success == false)
        {
            return Attempt.FailWithStatus<PagedModel<ILogEntry>?, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null);
        }

        PagedModel<ILogEntry> filteredLogs =
            GetFilteredLogs(logTimePeriod, filterExpression, logLevels, orderDirection, skip, take);

        return Attempt.SucceedWithStatus<PagedModel<ILogEntry>?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            filteredLogs);
    }

    /// <inheritdoc/>
    public virtual Task<Attempt<bool, LogViewerOperationStatus>> CanViewLogsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => CanViewLogsAsync(GetTimePeriod(startDate, endDate));

    /// <summary>
    /// Checks if the logs for the specified time period can be viewed.
    /// </summary>
    public abstract Task<Attempt<bool, LogViewerOperationStatus>> CanViewLogsAsync(LogTimePeriod logTimePeriod);


    /// <summary>
    ///     Returns a <see cref="LogTimePeriod" /> representation from a start and end date for filtering log files.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The LogTimePeriod object used to filter logs.</returns>
    protected virtual LogTimePeriod GetTimePeriod(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        if (startDate is null || endDate is null)
        {
            DateTime now = DateTime.Now;
            startDate ??= now.AddDays(-1);
            endDate ??= now;
        }

        return new LogTimePeriod(startDate.Value.LocalDateTime, endDate.Value.LocalDateTime);
    }

    /// <summary>
    /// Gets a filtered page of logs from storage based on the provided parameters.
    /// </summary>
    protected PagedModel<ILogEntry> GetFilteredLogs(
        LogTimePeriod logTimePeriod,
        string? filterExpression,
        string[]? logLevels,
        Direction orderDirection,
        int skip,
        int take)
    {
        IEnumerable<ILogEntry> logs = LogViewerRepository.GetLogs(logTimePeriod, filterExpression).ToArray();

        // This is user used the checkbox UI to toggle which log levels they wish to see
        // If an empty array or null - its implied all levels to be viewed
        if (logLevels?.Length > 0)
        {
            var logsAfterLevelFilters = new List<ILogEntry>();
            var validLogType = true;
            foreach (var level in logLevels)
            {
                // Check if level string is part of the LogEventLevel enum
                if (Enum.IsDefined(typeof(LogLevel), level))
                {
                    validLogType = true;
                    logsAfterLevelFilters.AddRange(logs.Where(x =>
                        string.Equals(x.Level.ToString(), level, StringComparison.InvariantCultureIgnoreCase)));
                }
                else
                {
                    validLogType = false;
                }
            }

            if (validLogType)
            {
                logs = logsAfterLevelFilters;
            }
        }

        return new PagedModel<ILogEntry>
        {
            Total = logs.Count(),
            Items = logs
                .OrderBy(l => l.Timestamp, orderDirection)
                .Skip(skip)
                .Take(take),
        };
    }
}
