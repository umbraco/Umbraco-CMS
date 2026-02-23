using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[ApiVersion("1.0")]
public class AncestorsDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;

    public AncestorsDocumentTypeItemController(IItemAncestorService itemAncestorService)
        => _itemAncestorService = itemAncestorService;

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<DocumentTypeItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of document type items.")]
    [EndpointDescription("Gets the ancestor chains for document type items identified by the provided Ids.")]
    public IActionResult Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<DocumentTypeItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<DocumentTypeItemResponseModel>> result = _itemAncestorService.GetAncestors(
            UmbracoObjectTypes.DocumentType,
            UmbracoObjectTypes.DocumentTypeContainer,
            ids,
            ancestors => ancestors.ToDictionary(
                a => a.Key,
                a => new DocumentTypeItemResponseModel { Id = a.Key, Name = a.Name ?? string.Empty }));

        return Ok(result);
    }
}
