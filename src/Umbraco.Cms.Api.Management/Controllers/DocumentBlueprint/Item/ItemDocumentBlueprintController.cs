using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Item;

[ApiVersion("1.0")]
public class ItemDocumentBlueprintController : DocumentBlueprintItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public ItemDocumentBlueprintController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _entityService = entityService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentBlueprintResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "id")] SortedSet<Guid> ids)
    {
        IEnumerable<IDocumentEntitySlim> documents = _entityService.GetAll(UmbracoObjectTypes.Document, ids.ToArray()).Select(x => x as IDocumentEntitySlim).WhereNotNull();
        IEnumerable<DocumentBlueprintResponseModel> responseModels = documents.Select(x => _documentPresentationFactory.CreateBlueprintItemResponseModel(x));
        return Ok(responseModels);
    }
}
