using System.Collections.ObjectModel;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Core.Services;

public class LogViewerService : ILogViewerService
{
    private const int FileSizeCap = 100;
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly ICoreScopeProvider _provider;
    private readonly ILoggingConfiguration _loggingConfiguration;
    private readonly ILogViewerRepository _logViewerRepository;

    public LogViewerService(
        ILogViewerQueryRepository logViewerQueryRepository,
        ICoreScopeProvider provider,
        ILoggingConfiguration loggingConfiguration,
        ILogViewerRepository logViewerRepository)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _provider = provider;
        _loggingConfiguration = loggingConfiguration;
        _logViewerRepository = logViewerRepository;
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<ILogEntry>?, LogViewerOperationStatus>> GetPagedLogsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        string[]? logLevels = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return Attempt.FailWithStatus<PagedModel<ILogEntry>?, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null);
        }


        PagedModel<ILogEntry> filteredLogs = GetFilteredLogs(logTimePeriod, filterExpression, logLevels, orderDirection, skip, take);

        return Attempt.SucceedWithStatus<PagedModel<ILogEntry>?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            filteredLogs);
    }

    /// <inheritdoc/>
    public async Task<PagedModel<ILogViewerQuery>> GetSavedLogQueriesAsync(int skip, int take)
    {
        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        ILogViewerQuery[] savedLogQueries = _logViewerQueryRepository.GetMany().ToArray();
        var pagedModel = new PagedModel<ILogViewerQuery>(savedLogQueries.Length, savedLogQueries.Skip(skip).Take(take));
        return await Task.FromResult(pagedModel);
    }

    /// <inheritdoc/>
    public async Task<ILogViewerQuery?> GetSavedLogQueryByNameAsync(string name)
    {
        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        return await Task.FromResult(_logViewerQueryRepository.GetByName(name));
    }

    /// <inheritdoc/>
    public async Task<Attempt<ILogViewerQuery?, LogViewerOperationStatus>> AddSavedLogQueryAsync(string name, string query)
    {
        ILogViewerQuery? logViewerQuery = await GetSavedLogQueryByNameAsync(name);

        if (logViewerQuery is not null)
        {
            return Attempt.FailWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(LogViewerOperationStatus.DuplicateLogSearch, null);
        }

        logViewerQuery = new LogViewerQuery(name, query);

        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        _logViewerQueryRepository.Save(logViewerQuery);

        return Attempt.SucceedWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(LogViewerOperationStatus.Success, logViewerQuery);
    }

    /// <inheritdoc/>
    public async Task<Attempt<ILogViewerQuery?, LogViewerOperationStatus>> DeleteSavedLogQueryAsync(string name)
    {
        ILogViewerQuery? logViewerQuery = await GetSavedLogQueryByNameAsync(name);

        if (logViewerQuery is null)
        {
            return Attempt.FailWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(LogViewerOperationStatus.NotFoundLogSearch, null);
        }

        using ICoreScope scope = _provider.CreateCoreScope(autoComplete: true);
        _logViewerQueryRepository.Delete(logViewerQuery);

        return Attempt.SucceedWithStatus<ILogViewerQuery?, LogViewerOperationStatus>(LogViewerOperationStatus.Success, logViewerQuery);
    }

    /// <inheritdoc/>
    public async Task<Attempt<bool, LogViewerOperationStatus>> CanViewLogsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);
        bool isAllowed = CanViewLogs(logTimePeriod);

        if (isAllowed == false)
        {
            return Attempt.FailWithStatus(LogViewerOperationStatus.CancelledByLogsSizeValidation, isAllowed);
        }

        return Attempt.SucceedWithStatus(LogViewerOperationStatus.Success, isAllowed);
    }

    /// <inheritdoc/>
    public async Task<Attempt<LogLevelCounts?, LogViewerOperationStatus>> GetLogLevelCountsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return Attempt.FailWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null);
        }

        LogLevelCounts counter = _logViewerRepository.GetLogCount(logTimePeriod);

        return Attempt.SucceedWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            counter);
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<LogTemplate>, LogViewerOperationStatus>> GetMessageTemplatesAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, int skip, int take)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return Attempt.FailWithStatus<PagedModel<LogTemplate>, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null!);
        }

        LogTemplate[] messageTemplates = _logViewerRepository.GetMessageTemplates(logTimePeriod);

        return Attempt.SucceedWithStatus(
            LogViewerOperationStatus.Success,
            new PagedModel<LogTemplate>(messageTemplates.Length, messageTemplates.Skip(skip).Take(take)));
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, LogLevel> GetLogLevelsFromSinks()
    {
        var configuredLogLevels = new Dictionary<string, LogLevel>
        {
            { "Global", GetGlobalMinLogLevel() },
            { "UmbracoFile", _logViewerRepository.RestrictedToMinimumLevel() },
        };

        return configuredLogLevels.AsReadOnly();
    }

    /// <inheritdoc/>
    public LogLevel GetGlobalMinLogLevel() => _logViewerRepository.GetGlobalMinLogLevel();

    /// <summary>
    ///     Returns a <see cref="LogTimePeriod" /> representation from a start and end date for filtering log files.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The LogTimePeriod object used to filter logs.</returns>
    private LogTimePeriod GetTimePeriod(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        if (startDate is null || endDate is null)
        {
            DateTime now = DateTime.Now;
            if (startDate is null)
            {
                startDate = now.AddDays(-1);
            }

            if (endDate is null)
            {
                endDate = now;
            }
        }

        return new LogTimePeriod(startDate.Value.LocalDateTime, endDate.Value.LocalDateTime);
    }

    /// <summary>
    ///     Returns a value indicating whether to stop a GET request that is attempting to fetch logs from a 1GB file.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter the logs.</param>
    /// <returns>The value whether or not you are able to view the logs.</returns>
    private bool CanViewLogs(LogTimePeriod logTimePeriod)
    {
        // Number of entries
        long fileSizeCount = 0;

        // foreach full day in the range - see if we can find one or more filenames that end with
        // yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
        for (DateTime day = logTimePeriod.StartTime.Date; day.Date <= logTimePeriod.EndTime.Date; day = day.AddDays(1))
        {
            // Filename ending to search for (As could be multiple)
            var filesToFind = GetSearchPattern(day);

            var filesForCurrentDay = Directory.GetFiles(_loggingConfiguration.LogDirectory, filesToFind);

            fileSizeCount += filesForCurrentDay.Sum(x => new FileInfo(x).Length);
        }

        // The GetLogSize call on JsonLogViewer returns the total file size in bytes
        // Check if the log size is not greater than 100Mb (FileSizeCap)
        var logSizeAsMegabytes = fileSizeCount / 1024 / 1024;
        return logSizeAsMegabytes <= FileSizeCap;
    }

    private PagedModel<ILogEntry> GetFilteredLogs(
        LogTimePeriod logTimePeriod,
        string? filterExpression,
        string[]? logLevels,
        Direction orderDirection,
        int skip,
        int take)
    {
        IEnumerable<ILogEntry> logs = _logViewerRepository.GetLogs(logTimePeriod, filterExpression).ToArray();

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

    private string GetSearchPattern(DateTime day) => $"*{day:yyyyMMdd}*.json";
}
