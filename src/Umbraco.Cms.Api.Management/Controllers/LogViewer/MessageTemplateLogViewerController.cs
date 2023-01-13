using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

public class MessageTemplateLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public MessageTemplateLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
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
    [ProducesResponseType(typeof(PagedViewModel<LogTemplateViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LogTemplateViewModel>>> AllMessageTemplates(
        int skip = 0,
        int take = 100,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Attempt<IEnumerable<LogTemplate>> messageTemplatesAttempt = _logViewerService.GetMessageTemplates(startDate, endDate);

        // We will need to stop the request if trying to do this on a 1GB file
        if (messageTemplatesAttempt.Success == false)
        {
            return await Task.FromResult(ValidationProblem("Unable to view logs, due to their size"));
        }

        IEnumerable<LogTemplate> messageTemplates = messageTemplatesAttempt
            .Result!
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<LogTemplateViewModel>>(messageTemplates)));
    }
}
