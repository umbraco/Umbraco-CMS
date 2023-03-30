using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

public class ItemDocumentItemController : DocumentItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _mapper;

    public ItemDocumentItemController(IEntityService entityService, IUmbracoMapper mapper)
    {
        _entityService = entityService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "key")] Guid[] keys, Guid? dataTypeKey = null, string? culture = null)
    {
        IEnumerable<IDocumentEntitySlim> documents = _entityService.GetAll(UmbracoObjectTypes.Document, keys).Select(x => x as IDocumentEntitySlim).Where(x => x is not null)!;
        List<DocumentItemResponseModel> documentItemResponseModels = _mapper.MapEnumerable<IDocumentEntitySlim, DocumentItemResponseModel>(documents);
        return Ok(documentItemResponseModels);
    }
}
