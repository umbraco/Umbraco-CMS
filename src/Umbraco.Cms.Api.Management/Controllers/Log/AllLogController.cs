using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Log;

public class AllLogController : LogControllerBase
{
    private readonly ILogViewer _logViewer;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllLogController(ILogViewer logViewer, IUmbracoMapper umbracoMapper)
        : base(logViewer)
    {
        _logViewer = logViewer;
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
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LogMessageViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LogMessageViewModel>>> All(
        int skip = 0,
        int take = 100,
        string orderDirection = "Descending",
        string? filterExpression = null,
        [FromQuery(Name = "logLevel")] string[]? logLevels = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        IEnumerable<LogMessage> result = Enumerable.Empty<LogMessage>();
        Direction direction = orderDirection.InvariantEquals("Descending") ? Direction.Descending : Direction.Ascending;

        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        // Passing the take value as the pageSize and keeping the pageNumber as default so we can get all elements
        IEnumerable<LogMessage>? logs = _logViewer
            .GetLogs(logTimePeriod, filterExpression: filterExpression, pageSize: take, orderDirection: direction, logLevels: logLevels)
            .Items;

        if (logs is not null)
        {
            result = logs
                .Skip(skip)
                .Take(take);
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<LogMessageViewModel>>(result)));
    }
}
