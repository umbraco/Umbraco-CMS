using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

public class ItemDocumentItemController : DocumentItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IUserStartNodeEntitiesService _userStartNodeEntitiesService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public ItemDocumentItemController(
        IEntityService entityService,
        IDataTypeService dataTypeService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _entityService = entityService;
        _dataTypeService = dataTypeService;
        _userStartNodeEntitiesService = userStartNodeEntitiesService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "key")] Guid[] keys, Guid? dataTypeKey = null, string? culture = null)
    {
        IEnumerable<IDocumentEntitySlim> documents = _entityService.GetAll(UmbracoObjectTypes.Document, keys).Select(x => x as IDocumentEntitySlim).Where(x => x is not null)!;

        // Filter start nodes
        if (dataTypeKey is not null)
        {
            if (_dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeKey.Value))
            {
                // FIXME: right now we're faking user id by just passing "-1"
                // We should use the backoffice security accessor once auth is in place.
                documents = _userStartNodeEntitiesService.UserAccessEntities(documents, new[] {"-1"}).Select(x => x.Entity as IDocumentEntitySlim).WhereNotNull();
            }
        }

        IEnumerable<DocumentItemResponseModel> documentItemResponseModels = documents.Select(x => _documentPresentationFactory.CreateItemResponseModel(x, culture));
        return Ok(documentItemResponseModels);
    }
}
