using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

/// <summary>
/// Controller responsible for retrieving ancestor chains for element items in the management API.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsElementItemController : ElementItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsElementItemController"/> class.
    /// </summary>
    /// <param name="itemAncestorService">Service used to perform bulk ancestor lookups.</param>
    public AncestorsElementItemController(IItemAncestorService itemAncestorService)
        => _itemAncestorService = itemAncestorService;

    /// <summary>
    /// Gets the ancestor chains for a collection of element items.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">The unique identifiers of the element items to retrieve ancestors for.</param>
    /// <returns>A collection of ancestor chains for the requested element items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<NamedItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of element items.")]
    [EndpointDescription("Gets the ancestor chains for element items identified by the provided Ids.")]
    public async Task<IActionResult> Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<NamedItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<NamedItemResponseModel>> result = await _itemAncestorService.GetAncestorsAsync(
            UmbracoObjectTypes.Element,
            UmbracoObjectTypes.ElementContainer,
            ids);

        return Ok(result);
    }
}
