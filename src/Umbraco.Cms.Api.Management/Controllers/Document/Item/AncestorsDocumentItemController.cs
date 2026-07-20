using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class AncestorsDocumentItemController : DocumentItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public AncestorsDocumentItemController(
        IItemAncestorService itemAncestorService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _itemAncestorService = itemAncestorService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<DocumentItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of document items.")]
    [EndpointDescription("Gets the ancestor chains for document items identified by the provided Ids.")]
    public async Task<IActionResult> Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<DocumentItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<DocumentItemResponseModel>> result = await _itemAncestorService.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            ids,
            ancestors => Task.FromResult(
                ancestors
                    .OfType<IDocumentEntitySlim>()
                    .Select(_documentPresentationFactory.CreateItemResponseModel)));

        return Ok(result);
    }
}
