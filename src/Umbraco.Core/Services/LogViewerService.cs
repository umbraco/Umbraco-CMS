using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Represents a service for viewing logs in Umbraco.
/// </summary>
public class LogViewerService : LogViewerServiceBase
{
    private const int FileSizeCap = 100;
    private readonly ILoggingConfiguration _loggingConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerService"/> class.
    /// </summary>
    public LogViewerService(
        ILogViewerQueryRepository logViewerQueryRepository,
        ICoreScopeProvider provider,
        ILoggingConfiguration loggingConfiguration,
        ILogViewerRepository logViewerRepository)
        : base(
            logViewerQueryRepository,
            provider,
            logViewerRepository)
    {
        _loggingConfiguration = loggingConfiguration;
    }

    /// <inheritdoc/>
    protected override string LoggerName => "UmbracoFile";

    /// <inheritdoc/>
    public override Task<Attempt<bool, LogViewerOperationStatus>> CanViewLogsAsync(LogTimePeriod logTimePeriod)
    {
        bool isAllowed = CanViewLogs(logTimePeriod);

        if (isAllowed == false)
        {
            return Task.FromResult(Attempt.FailWithStatus(
                LogViewerOperationStatus.CancelledByLogsSizeValidation,
                isAllowed));
        }

        return Task.FromResult(Attempt.SucceedWithStatus(LogViewerOperationStatus.Success, isAllowed));
    }

    /// <summary>
    ///     Returns a value indicating whether to stop a GET request that is attempting to fetch logs from a 1GB file.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter the logs.</param>
    /// <returns>Whether you are able to view the logs.</returns>
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

    private static string GetSearchPattern(DateTime day) => $"*{day:yyyyMMdd}*.json";
}
