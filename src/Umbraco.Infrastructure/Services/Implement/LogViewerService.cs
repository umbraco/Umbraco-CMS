using System.Collections.ObjectModel;
using Serilog.Events;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Services.Implement;

// FIXME: Get rid of ILogViewer and ILogLevelLoader dependencies (as they are obsolete)
// and fix the implementation of the methods using it
public class LogViewerService : ILogViewerService
{
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly ILogViewer _logViewer;
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly ICoreScopeProvider _provider;
    private readonly IJsonSerializer _jsonSerializer;

    public LogViewerService(
        ILogViewerQueryRepository logViewerQueryRepository,
        ILogViewer logViewer,
        ILogLevelLoader logLevelLoader,
        ICoreScopeProvider provider,
        IJsonSerializer jsonSerializer)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _logViewer = logViewer;
        _logLevelLoader = logLevelLoader;
        _provider = provider;
        _jsonSerializer = jsonSerializer;
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

        PagedModel<LogMessage> logMessages =
            _logViewer.GetLogsAsPagedModel(logTimePeriod, skip, take, orderDirection, filterExpression, logLevels);

        var logEntries = new PagedModel<ILogEntry>(logMessages.Total, logMessages.Items.Select(x => ToLogEntry(x)));

        return Attempt.SucceedWithStatus<PagedModel<ILogEntry>?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            logEntries);
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

        return Attempt.SucceedWithStatus<LogLevelCounts?, LogViewerOperationStatus>(
            LogViewerOperationStatus.Success,
            _logViewer.GetLogLevelCounts(logTimePeriod));
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

        LogTemplate[] messageTemplates = _logViewer.GetMessageTemplates(logTimePeriod).ToArray();

        return Attempt.SucceedWithStatus(
            LogViewerOperationStatus.Success,
            new PagedModel<LogTemplate>(messageTemplates.Length, messageTemplates.Skip(skip).Take(take)));
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, LogLevel> GetLogLevelsFromSinks()
    {
        ReadOnlyDictionary<string, LogEventLevel?> configuredLogLevels = _logLevelLoader.GetLogLevelsFromSinks();

        return configuredLogLevels.ToDictionary(logLevel => logLevel.Key, logLevel => Enum.Parse<LogLevel>(logLevel.Value!.ToString()!)).AsReadOnly();
    }

    /// <inheritdoc/>
    public LogLevel GetGlobalMinLogLevel()
    {
        LogEventLevel? serilogLogLevel = _logLevelLoader.GetGlobalMinLogLevel();

        return Enum.Parse<LogLevel>(serilogLogLevel!.ToString()!);
    }

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
        // Check if the interface can deal with large files
        if (_logViewer.CanHandleLargeLogs)
        {
            return true;
        }

        return _logViewer.CheckCanOpenLogs(logTimePeriod);
    }

    private ILogEntry ToLogEntry(LogMessage logMessage)
    {
        return new LogEntry()
        {
            Timestamp = logMessage.Timestamp,
            Level = Enum.Parse<LogLevel>(logMessage.Level.ToString()),
            MessageTemplateText = logMessage.MessageTemplateText,
            RenderedMessage = logMessage.RenderedMessage,
            Properties = MapLogMessageProperties(logMessage.Properties),
            Exception = logMessage.Exception
        };
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
}
