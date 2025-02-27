using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiVersion("1.0")]
public class LogLevelCountLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public LogLevelCountLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets the count for each log level from the logs for a specific date range.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The log level counts from the (filtered) logs.</returns>
    [HttpGet("level-count")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LogLevelCountsReponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogLevelCounts(
        CancellationToken cancellationToken,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        Attempt<LogLevelCounts?, LogViewerOperationStatus> logLevelCountsAttempt =
            await _logViewerService.GetLogLevelCountsAsync(startDate, endDate);

        if (logLevelCountsAttempt.Success)
        {
            return Ok(_umbracoMapper.Map<LogLevelCountsReponseModel>(logLevelCountsAttempt.Result));
        }

        return LogViewerOperationStatusResult(logLevelCountsAttempt.Status);
    }
}
