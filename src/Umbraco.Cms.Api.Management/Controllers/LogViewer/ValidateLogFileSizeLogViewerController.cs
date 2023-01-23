using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

public class ValidateLogFileSizeLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;

    public ValidateLogFileSizeLogViewerController(ILogViewerService logViewerService) => _logViewerService = logViewerService;

    /// <summary>
    ///     Gets a value indicating whether or not you are able to view logs for a specified date range.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The boolean result.</returns>
    [HttpGet("validate-logs-size")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanViewLogs(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await Task.FromResult(Ok(_logViewerService.CanViewLogsAsync(startDate, endDate).Result));
    }
}
