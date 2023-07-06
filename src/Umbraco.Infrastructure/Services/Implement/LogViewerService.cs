using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using Umbraco.Extensions;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Core.Services.Implement;

public class LogViewerService : ILogViewerService
{
    private const int FileSizeCap = 100;
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly ICoreScopeProvider _provider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly UmbracoFileConfiguration _umbracoFileConfig;
    private readonly ILogger<LogViewerService> _logger;
    private readonly ILoggingConfiguration _loggingConfiguration;
    private readonly ILogViewerRepository _logViewerRepository;

    [Obsolete("Use the constructor without ILogLevelLoader instead, Scheduled for removal in Umbraco 15.")]
    public LogViewerService(
        ILogViewerQueryRepository logViewerQueryRepository,
        ILogViewer logViewer,
        ILogLevelLoader logLevelLoader,
        ICoreScopeProvider provider,
        IJsonSerializer jsonSerializer,
        UmbracoFileConfiguration umbracoFileConfig)
        : this(
            logViewerQueryRepository,
            provider,
            jsonSerializer,
            umbracoFileConfig,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<LogViewerService>>(),
            StaticServiceProvider.Instance.GetRequiredService<ILoggingConfiguration>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogViewerRepository>())
    {
    }

    public LogViewerService(
        ILogViewerQueryRepository logViewerQueryRepository,
        ICoreScopeProvider provider,
        IJsonSerializer jsonSerializer,
        UmbracoFileConfiguration umbracoFileConfig,
        ILogger<LogViewerService> logger,
        ILoggingConfiguration loggingConfiguration,
        ILogViewerRepository logViewerRepository)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _provider = provider;
        _jsonSerializer = jsonSerializer;
        _umbracoFileConfig = umbracoFileConfig;
        _logger = logger;
        _loggingConfiguration = loggingConfiguration;
        _logViewerRepository = logViewerRepository;
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<ILogEntry>?, LogViewerOperationStatus>> GetPagedLogsAsync(
        DateTime? startDate,
        DateTime? endDate,
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
    public async Task<Attempt<bool, LogViewerOperationStatus>> CanViewLogsAsync(DateTime? startDate, DateTime? endDate)
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
    public async Task<Attempt<LogLevelCounts?, LogViewerOperationStatus>> GetLogLevelCountsAsync(DateTime? startDate, DateTime? endDate)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return Attempt.FailWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null);
        }

        var counter = new CountingFilter();
        GetLogs(logTimePeriod, counter, 0, int.MaxValue);

        return Attempt.SucceedWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            counter.Counts);
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<LogTemplate>, LogViewerOperationStatus>> GetMessageTemplatesAsync(DateTime? startDate, DateTime? endDate, int skip, int take)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return Attempt.FailWithStatus<PagedModel<LogTemplate>, LogViewerOperationStatus>(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                null!);
        }

        var messageTemplates = new MessageTemplateFilter();
        GetLogs(logTimePeriod, messageTemplates, 0, int.MaxValue);

        LogTemplate[] templates = messageTemplates.Counts
            .Select(x => new LogTemplate { MessageTemplate = x.Key, Count = x.Value })
            .OrderByDescending(x => x.Count).ToArray();

        return Attempt.SucceedWithStatus(
            LogViewerOperationStatus.Success,
            new PagedModel<LogTemplate>(templates.Length, templates.Skip(skip).Take(take)));
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, LogLevel> GetLogLevelsFromSinks()
    {
        var configuredLogLevels = new Dictionary<string, LogEventLevel?>
        {
            { "Global", GetGlobalLogLevelEventMinLevel() },
            { "UmbracoFile", _umbracoFileConfig.RestrictedToMinimumLevel },
        };

        return configuredLogLevels.ToDictionary(logLevel => logLevel.Key, logLevel => Enum.Parse<LogLevel>(logLevel.Value!.ToString()!)).AsReadOnly();
    }

    /// <inheritdoc/>
    public LogLevel GetGlobalMinLogLevel()
    {
        LogEventLevel logLevel = GetGlobalLogLevelEventMinLevel();

        return Enum.Parse<LogLevel>(logLevel.ToString());
    }


    private LogEventLevel GetGlobalLogLevelEventMinLevel() =>
        Enum.GetValues(typeof(LogEventLevel))
            .Cast<LogEventLevel>()
            .Where(Log.IsEnabled)
            .DefaultIfEmpty(LogEventLevel.Information).Min();

    /// <summary>
    ///     Returns a <see cref="LogTimePeriod" /> representation from a start and end date for filtering log files.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The LogTimePeriod object used to filter logs.</returns>
    private LogTimePeriod GetTimePeriod(DateTime? startDate, DateTime? endDate)
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

        return new LogTimePeriod(startDate.Value, endDate.Value);
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

    private IReadOnlyDictionary<string, string?> MapLogMessageProperties(IReadOnlyDictionary<string, LogEventPropertyValue>? properties)
    {
        var result = new Dictionary<string, string?>();

        if (properties is not null)
        {
            foreach (KeyValuePair<string, LogEventPropertyValue> property in properties)
            {
                string? value;


                if (property.Value is ScalarValue scalarValue)
                {
                    value = scalarValue.Value?.ToString();
                }
                else if (property.Value is StructureValue structureValue)
                {
                    var textWriter = new StringWriter();
                    structureValue.Render(textWriter);
                    value = textWriter.ToString();
                }
                else
                {
                    value = _jsonSerializer.Serialize(property.Value);
                }

                result.Add(property.Key, value);
            }
        }

        return result.AsReadOnly();
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
                if (Enum.IsDefined(typeof(LogEventLevel), level))
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
