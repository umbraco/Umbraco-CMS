using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

/// <summary>
/// Controller responsible for handling API operations related to retrieving sibling member types within the member type tree structure.
/// </summary>
public class SiblingMemberTypeTreeController : MemberTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingMemberTypeTreeController"/> class, which manages the tree structure for sibling member types in the Umbraco backoffice.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="memberTypeService">Service for managing member types.</param>
    public SiblingMemberTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, memberTypeService)
    {
    }

    /// <summary>
    /// Retrieves a subset of member type tree items that are siblings of the specified member type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the member type whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target member type.</param>
    /// <param name="after">The number of sibling items to include after the target member type.</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folder items are included in the result; otherwise, all sibling items are included.</param>
    /// <returns>A <see cref="SubsetViewModel{T}"/> containing the sibling <see cref="MemberTypeTreeItemResponseModel"/> items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets sibling member types in the tree.")]
    [EndpointDescription("Gets a collection of member type tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<MemberTypeTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
