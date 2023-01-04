using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiController]
[VersionedApiBackOfficeRoute("log")]
[ApiExplorerSettings(GroupName = "Log")]
[ApiVersion("1.0")]
public abstract class LogControllerBase : ManagementApiControllerBase
{
    private readonly ILogViewer _logViewer;

    protected LogControllerBase(ILogViewer logViewer)
    {
        _logViewer = logViewer;
    }

    /// <summary>
    ///     Returns a value indicating whether to stop a GET request that is attempting to fetch logs from a 1GB file.
    /// </summary>
    /// <param name="logTimePeriod">The time period to filter the logs.</param>
    /// <returns>The value whether or not you are able to view the logs.</returns>
    protected virtual bool CanViewLogs(LogTimePeriod logTimePeriod)
    {
        // Check if the interface can deal with large files
        if (_logViewer.CanHandleLargeLogs)
        {
            return true;
        }

        return _logViewer.CheckCanOpenLogs(logTimePeriod);
    }

    /// <summary>
    ///     Returns a <see cref="LogTimePeriod" /> representation from a start and end date for filtering log files.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The LogTimePeriod object used to filter logs.</returns>
    protected virtual LogTimePeriod GetTimePeriod(DateTime? startDate, DateTime? endDate)
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
}
