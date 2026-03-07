using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

    /// <summary>
    /// API controller that handles operations related to the root node of the member type tree structure in Umbraco CMS management.
    /// Provides endpoints for retrieving and managing the root of the member type hierarchy.
    /// </summary>
[ApiVersion("1.0")]
public class RootMemberTypeTreeController : MemberTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootMemberTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for operations on entities within Umbraco.</param>
    /// <param name="memberTypeService">Service used for managing member types in Umbraco.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootMemberTypeTreeController(IEntityService entityService, IMemberTypeService memberTypeService)
        : base(entityService, memberTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootMemberTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="memberTypeService">Service for managing member types in the Umbraco CMS.</param>
    [ActivatorUtilitiesConstructor]
    public RootMemberTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, memberTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of member type items from the root of the member type tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folder items will be included in the result; otherwise, all member type items are returned.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="PagedViewModel{T}"/> of <see cref="MemberTypeTreeItemResponseModel"/> representing the member type items at the root level.
    /// </returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member type items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of member type items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<MemberTypeTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
