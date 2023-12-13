using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Composition;

[ApiVersion("1.0")]
public class ReferencesCompositionController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ReferencesCompositionController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/composition-references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ContentTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompositionReferences(Guid id)
    {
        var contentType = await _contentTypeService.GetAsync(id); // NB: different for media/member (media/member service)

        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        IEnumerable<IContentType> composedOf = _contentTypeService.GetComposedOf(contentType.Id); // NB: different for media/member (media/member service)
        List<ContentTypeCompositionResponseModel> responseModels = _umbracoMapper.MapEnumerable<IContentType, ContentTypeCompositionResponseModel>(composedOf);

        return Ok(responseModels);
    }
}
