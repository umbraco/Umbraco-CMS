using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

    /// <summary>
    /// Controller responsible for handling operations related to the ancestor tree structure of member types in the management API.
    /// </summary>
[ApiVersion("1.0")]
public class AncestorsMemberTypeTreeController : MemberTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsMemberTypeTreeController"/> class, which provides API endpoints for retrieving ancestor member types in the tree structure.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    public AncestorsMemberTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, memberTypeService)
    {
    }

    /// <summary>
    /// Retrieves a collection of ancestor member type items for the specified descendant member type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantId">The unique identifier of the descendant member type whose ancestors are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of ancestor <see cref="MemberTypeTreeItemResponseModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor member type items.")]
    [EndpointDescription("Gets a collection of member type items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<MemberTypeTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
