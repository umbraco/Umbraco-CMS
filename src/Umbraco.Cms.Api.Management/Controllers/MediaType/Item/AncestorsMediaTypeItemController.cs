using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiVersion("1.0")]
public class AncestorsMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;

    public AncestorsMediaTypeItemController(IItemAncestorService itemAncestorService)
        => _itemAncestorService = itemAncestorService;

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<MediaTypeItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of media type items.")]
    [EndpointDescription("Gets the ancestor chains for media type items identified by the provided Ids.")]
    public IActionResult Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<MediaTypeItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<MediaTypeItemResponseModel>> result = _itemAncestorService.GetAncestors(
            UmbracoObjectTypes.MediaType,
            UmbracoObjectTypes.MediaTypeContainer,
            ids,
            ancestors => ancestors.ToDictionary(
                a => a.Key,
                a => new MediaTypeItemResponseModel { Id = a.Key, Name = a.Name ?? string.Empty }));

        return Ok(result);
    }
}
