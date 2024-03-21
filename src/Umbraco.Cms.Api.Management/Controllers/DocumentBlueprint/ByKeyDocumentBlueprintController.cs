using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[ApiVersion("1.0")]
public class ByKeyDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public ByKeyDocumentBlueprintController(IContentBlueprintEditingService contentBlueprintEditingService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        IContent? blueprint = await _contentBlueprintEditingService.GetAsync(id);
        if (blueprint == null)
        {
            return DocumentBlueprintNotFound();
        }

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(blueprint);
        return Ok(model);
    }
}
