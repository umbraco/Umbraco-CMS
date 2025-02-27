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

[ApiVersion("1.0")]
public class AllMessageTemplateLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllMessageTemplateLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all log message templates for a specific date range.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="startDate">The start date for the date range (can be null).</param>
    /// <param name="endDate">The end date for the date range (can be null).</param>
    /// <returns>The paged result of the log message templates from the given time period.</returns>
    [HttpGet("message-template")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<LogTemplateResponseModel>), StatusCodes.Status200OK)]
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
