using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

    /// <summary>
    /// API controller responsible for managing template resources identified by their unique key.
    /// Provides endpoints for operations such as retrieval, update, and deletion of templates.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ITemplatePresentationFactory _templatePresentationFactory;

    public ByKeyTemplateController(
        ITemplateService templateService,
        ITemplatePresentationFactory templatePresentationFactory)
    {
        _templateService = templateService;
        _templatePresentationFactory = templatePresentationFactory;
    }

    /// <summary>
    /// Retrieves a template by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the template to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the <see cref="TemplateResponseModel"/> if the template is found; otherwise, a not found result.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a template.")]
    [EndpointDescription("Gets a template identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        ITemplate? template = await _templateService.GetAsync(id);
        return template == null
            ? TemplateNotFound()
            : Ok(await _templatePresentationFactory.CreateTemplateResponseModelAsync(template));
    }
}
