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
    private readonly IContentService _contentService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public ByKeyDocumentBlueprintController(IContentService contentService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _contentService = contentService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK),
     ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        IContent? content = _contentService.GetBlueprintById(id);

        if (content == null)
        {
            return DocumentBlueprintNotFound();
        }

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(content);
        return Ok(model);
    }
}
