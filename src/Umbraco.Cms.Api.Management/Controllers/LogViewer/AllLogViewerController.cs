using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

public class AllLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated grouped list of all logs for a specific date range.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="orderDirection">
    ///     By default this will be ordered descending (newest items first).
    /// </param>
    /// <param name="filterExpression">The query expression to filter on.</param>
    /// <param name="logLevels">The log levels for which to retrieve the log messages.</param>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The paged result of logs from the given time period.</returns>
    [HttpGet("log")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LogMessageViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LogMessageViewModel>>> AllLogs(
        int skip = 0,
        int take = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        [FromQuery(Name = "logLevel")] string[]? logLevels = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Attempt<PagedModel<ILogEntry>> logsAttempt = _logViewerService.GetPagedLogs(startDate, endDate, skip, take,
            orderDirection, filterExpression, logLevels);

        // We will need to stop the request if trying to do this on a 1GB file
        if (logsAttempt.Success == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<LogMessageViewModel>>(logsAttempt.Result)));
    }
}
