using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Log;

public class EventLevelCountLogController : LogControllerBase
{
    private readonly ILogViewer _logViewer;
    private readonly IUmbracoMapper _umbracoMapper;

    public EventLevelCountLogController(ILogViewer logViewer, IUmbracoMapper umbracoMapper)
        : base(logViewer)
    {
        _logViewer = logViewer;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets the count for each log level from the logs for a specific date range.
    /// </summary>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The log level counts from the (filtered) logs.</returns>
    [HttpGet("level/count")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LogLevelCountsViewModel>> LogLevelCounts(DateTime? startDate = null, DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        LogLevelCounts logLevelCounts = _logViewer.GetLogLevelCounts(logTimePeriod);

        return await Task.FromResult(Ok(_umbracoMapper.Map<LogLevelCountsViewModel>(logLevelCounts)));
    }
}
