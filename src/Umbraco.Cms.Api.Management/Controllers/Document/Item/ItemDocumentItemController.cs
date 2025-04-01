using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class ItemDocumentItemController : DocumentItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public ItemDocumentItemController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _entityService = entityService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<DocumentItemResponseModel>()));
        }

        IEnumerable<IDocumentEntitySlim> documents = _entityService
            .GetAll(UmbracoObjectTypes.Document, ids.ToArray())
            .OfType<IDocumentEntitySlim>();

        IEnumerable<DocumentItemResponseModel> documentItemResponseModels = documents.Select(_documentPresentationFactory.CreateItemResponseModel);
        return Task.FromResult<IActionResult>(Ok(documentItemResponseModels));
    }
}
