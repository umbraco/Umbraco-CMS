using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

[ApiVersion("1.0")]
public class AncestorsMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;

    public AncestorsMemberTypeItemController(IItemAncestorService itemAncestorService)
        => _itemAncestorService = itemAncestorService;

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<MemberTypeItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of member type items.")]
    [EndpointDescription("Gets the ancestor chains for member type items identified by the provided Ids.")]
    public IActionResult Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<MemberTypeItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<MemberTypeItemResponseModel>> result = _itemAncestorService.GetAncestors(
            UmbracoObjectTypes.MemberType,
            UmbracoObjectTypes.MemberTypeContainer,
            ids,
            ancestors => ancestors.ToDictionary(
                a => a.Key,
                a => new MemberTypeItemResponseModel { Id = a.Key, Name = a.Name ?? string.Empty }));

        return Ok(result);
    }
}
