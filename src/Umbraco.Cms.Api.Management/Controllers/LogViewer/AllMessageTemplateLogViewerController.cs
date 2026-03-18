using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

/// <summary>
/// Provides API endpoints for viewing logs that use all message templates.
/// </summary>
[ApiVersion("1.0")]
public class AllMessageTemplateLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllMessageTemplateLogViewerController"/> class.
    /// </summary>
    /// <param name="logViewerService">An instance of <see cref="ILogViewerService"/> used to interact with log data.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public AllMessageTemplateLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all log message templates for a specific date range.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The paged result of the log message templates from the given time period.</returns>
    [HttpGet("message-template")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<LogTemplateResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of log message templates.")]
    [EndpointDescription("Gets a paginated collection of unique message templates found in the logs.")]
    public async Task<IActionResult> AllMessageTemplates(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        Attempt<PagedModel<LogTemplate>, LogViewerOperationStatus> messageTemplatesAttempt = await _logViewerService.GetMessageTemplatesAsync(startDate, endDate, skip, take);

        if (messageTemplatesAttempt.Success)
        {
            var viewModel = new PagedViewModel<LogTemplateResponseModel>
            {
                Total = messageTemplatesAttempt.Result.Total,
                Items = _umbracoMapper.MapEnumerable<LogTemplate, LogTemplateResponseModel>(messageTemplatesAttempt.Result.Items)
            };

            return Ok(viewModel);
        }

        return LogViewerOperationStatusResult(messageTemplatesAttempt.Status);
    }
}
