using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[ApiVersion("1.0")]
public class ItemDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _mapper;

    public ItemDocumentTypeItemController(IContentTypeService contentTypeService, IUmbracoMapper mapper)
    {
        _contentTypeService = contentTypeService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IContentType> contentTypes = _contentTypeService.GetAll(ids);
        List<DocumentTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IContentType, DocumentTypeItemResponseModel>(contentTypes);
        return Ok(responseModels);
    }
}
