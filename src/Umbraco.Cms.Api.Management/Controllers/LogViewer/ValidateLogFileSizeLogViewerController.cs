using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiVersion("1.0")]
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CanViewLogs(
        CancellationToken cancellationToken,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        Attempt<bool, LogViewerOperationStatus> result = await _logViewerService.CanViewLogsAsync(startDate, endDate);

        if (result.Success)
        {
            return Ok();
        }

        return LogViewerOperationStatusResult(result.Status);
    }
}
