using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiVersion("1.0")]
public class AncestorsMemberItemController : MemberItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public AncestorsMemberItemController(
        IItemAncestorService itemAncestorService,
        IMemberPresentationFactory memberPresentationFactory)
    {
        _itemAncestorService = itemAncestorService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<MemberItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of member items.")]
    [EndpointDescription("Gets the ancestor chains for member items identified by the provided Ids.")]
    public async Task<IActionResult> Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<MemberItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<MemberItemResponseModel>> result = await _itemAncestorService.GetAncestorsAsync(
            UmbracoObjectTypes.Member,
            null,
            ids,
            ancestors => Task.FromResult(
                ancestors
                    .OfType<IMemberEntitySlim>()
                    .Select(_memberPresentationFactory.CreateItemResponseModel)));

        return Ok(result);
    }
}
