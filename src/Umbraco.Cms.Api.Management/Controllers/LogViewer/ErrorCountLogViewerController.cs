using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

public class ErrorCountLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewer _logViewer;

    public ErrorCountLogViewerController(ILogViewer logViewer)
        : base(logViewer) => _logViewer = logViewer;

    /// <summary>
    ///     Gets the number of errors from the logs for a specific date range.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The error count from the (filtered) logs.</returns>
    [HttpGet("error/count")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> ErrorCount(DateTime? startDate = null, DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        return await Task.FromResult(Ok(_logViewer.GetNumberOfErrors(logTimePeriod)));
    }
}
