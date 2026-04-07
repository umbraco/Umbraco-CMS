using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

/// <summary>
/// Controller that validates log file sizes in the Log Viewer.
/// </summary>
[ApiVersion("1.0")]
public class ValidateLogFileSizeLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateLogFileSizeLogViewerController"/> class, which is responsible for validating the size of log files using the provided log viewer service.
    /// </summary>
    /// <param name="logViewerService">The service used to interact with and validate log files.</param>
    public ValidateLogFileSizeLogViewerController(ILogViewerService logViewerService) => _logViewerService = logViewerService;

    /// <summary>
    ///     Gets a value indicating whether or not you are able to view logs for a specified date range.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The boolean result.</returns>
    [HttpGet("validate-logs-size")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Validates if logs can be viewed.")]
    [EndpointDescription("Checks if the log files are within the size limit and can be viewed.")]
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
