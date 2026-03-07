using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member.References;

/// <summary>
/// Provides API endpoints for managing members that are referenced as descendants.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedDescendantsMemberController : MemberControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedDescendantsMemberController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service for handling tracked references with skip and take operations.</param>
    /// <param name="umbracoMapper">The <see cref="IUmbracoMapper"/> instance used for mapping entities.</param>
    public ReferencedDescendantsMemberController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Gets a paged list of descendant members referenced by the specified member.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member whose referenced descendants are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{ReferenceByIdModel}"/> of referenced descendant members.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ReferencedDescendants2</c> instead. Scheduled for removal in Umbraco 19.
    /// </remarks>
    [Obsolete("Use the ReferencedDescendants2 action method instead. Scheduled for removal in Umbraco 19, when ReferencedDescendants2 will be renamed back to ReferencedDescendants.")]
    [NonAction]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> ReferencedDescendants(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, skip, take, true);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItems.Items),
        };

        return pagedViewModel;
    }

/// <summary>
///     Retrieves a paginated list of descendant member nodes of the specified member that are referenced in any relation.
/// </summary>
/// <remarks>
///     This method is typically used when deleting or unpublishing a member to determine if it has any descendant members that participate as children in any kind of relation.
///     It identifies descendant members that are referenced in relations, helping to ensure referential integrity before performing destructive operations.
/// </remarks>
/// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
/// <param name="id">The unique identifier of the member whose referenced descendants are to be retrieved.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set.</param>
/// <param name="take">The maximum number of items to return.</param>
/// <returns>A paginated collection of referenced descendant members.</returns>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a paginated collection of referenced descendant members.")]
    [EndpointDescription("Gets a paginated collection of descendant members that are referenced in relations for the member identified by the provided Id.")]
    public async Task<IActionResult> ReferencedDescendants2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, UmbracoObjectTypes.Member, skip, take, true);

        if (relationItemsAttempt.Success is false)
        {
            return GetReferencesOperationStatusResult(relationItemsAttempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItemsAttempt.Result.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItemsAttempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
