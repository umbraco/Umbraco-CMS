using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class AncestorsMediaItemController : MediaItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    public AncestorsMediaItemController(
        IItemAncestorService itemAncestorService,
        IMediaPresentationFactory mediaPresentationFactory)
    {
        _itemAncestorService = itemAncestorService;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<MediaItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of media items.")]
    [EndpointDescription("Gets the ancestor chains for media items identified by the provided Ids.")]
    public async Task<IActionResult> Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<MediaItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<MediaItemResponseModel>> result = await _itemAncestorService.GetAncestorsAsync(
            UmbracoObjectTypes.Media,
            null,
            ids,
            ancestors => Task.FromResult(
                ancestors
                    .OfType<IMediaEntitySlim>()
                    .Select(_mediaPresentationFactory.CreateItemResponseModel)));

        return Ok(result);
    }
}
