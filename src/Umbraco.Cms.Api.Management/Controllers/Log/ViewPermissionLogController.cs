using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.Log;

public class ViewPermissionLogController : LogControllerBase
{
    public ViewPermissionLogController(ILogViewer logViewer)
        : base(logViewer)
    {
    }

    /// <summary>
    ///     Gets a value indicating whether or not you are able to view logs for a specified date range.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The boolean result.</returns>
    [HttpGet("view-permission")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanViewLogs(DateTime? startDate = null, DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        return await Task.FromResult(Ok(CanViewLogs(logTimePeriod)));
    }
}
