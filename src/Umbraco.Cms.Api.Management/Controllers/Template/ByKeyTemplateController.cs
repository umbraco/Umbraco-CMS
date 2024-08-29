using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

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

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        ITemplate? template = await _templateService.GetAsync(id);
        return template == null
            ? TemplateNotFound()
            : Ok(await _templatePresentationFactory.CreateTemplateResponseModelAsync(template));
    }
}
