using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

/// <summary>
/// Controller responsible for retrieving and managing the child nodes of member types in the tree structure within the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenMemberTypeTreeController : MemberTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenMemberTypeTreeController"/> class, which manages the retrieval of child member types in the member type tree.
    /// </summary>
    /// <param name="entityService">The service used to interact with entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply additional flags or metadata for entities.</param>
    /// <param name="memberTypeService">The service used to manage member types.</param>
    public ChildrenMemberTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, memberTypeService)
    { }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member type tree child items.")]
    [EndpointDescription("Gets a paginated collection of member type tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<MemberTypeTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
