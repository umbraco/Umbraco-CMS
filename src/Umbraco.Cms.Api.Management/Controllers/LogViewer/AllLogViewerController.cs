using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Models;
using LogLevel = Umbraco.Cms.Core.Logging.LogLevel;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiVersion("1.0")]
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
    ///     Gets a paginated list of all logs for a specific date range.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="orderDirection">
    ///     By default this will be ordered descending (newest items first).
    /// </param>
    /// <param name="filterExpression">The query expression to filter on (can be null).</param>
    /// <param name="logLevels">The log levels for which to retrieve the log messages (can be null).</param>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The paged result of the logs from the given time period.</returns>
    [HttpGet("log")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LogMessageResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllLogs(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        Direction orderDirection = Direction.Descending,
        string? filterExpression = null,
        [FromQuery(Name = "logLevel")] LogLevel[]? logLevels = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        var levels = logLevels?.Select(l => l.ToString()).ToArray();

        Attempt<PagedModel<ILogEntry>?, LogViewerOperationStatus> logsAttempt =
            await _logViewerService.GetPagedLogsAsync(startDate, endDate, skip, take, orderDirection, filterExpression, levels);

        if (logsAttempt.Success)
        {
            var viewModel = new PagedViewModel<LogMessageResponseModel>
            {
                Total = logsAttempt.Result!.Total,
                Items = _umbracoMapper.MapEnumerable<ILogEntry, LogMessageResponseModel>(logsAttempt.Result.Items)
            };

            return Ok(viewModel);
        }

        return LogViewerOperationStatusResult(logsAttempt.Status);
    }
}
