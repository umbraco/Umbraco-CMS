using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LogLevelCountsViewModel>> LogLevelCounts(DateTime? startDate = null, DateTime? endDate = null)
    {
        Attempt<LogLevelCounts> logLevelCountsAttempt = _logViewerService.GetLogLevelCounts(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (logLevelCountsAttempt.Success == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<LogLevelCountsViewModel>(logLevelCountsAttempt.Result)));
    }
}
