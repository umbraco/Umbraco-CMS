using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

/// <summary>
/// Controller responsible for managing and exposing operations related to the ancestors of dictionary items in the dictionary tree.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsDictionaryTreeController : DictionaryTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDictionaryTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within Umbraco.</param>
    /// <param name="dictionaryItemService">Service used for managing dictionary items in the Umbraco dictionary tree.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDictionaryTreeController"/> class, which handles operations related to retrieving ancestor dictionary tree items.
    /// </summary>
    /// <param name="entityService">The service used for entity operations.</param>
    /// <param name="flagProviders">A collection of providers for entity flags.</param>
    /// <param name="dictionaryItemService">The service used for dictionary item operations.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsDictionaryTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    /// <summary>
    /// Retrieves all ancestor dictionary items for the specified descendant item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantId">The unique identifier of the descendant dictionary item whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="NamedEntityTreeItemResponseModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor dictionary items.")]
    [EndpointDescription("Gets a collection of dictionary items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
