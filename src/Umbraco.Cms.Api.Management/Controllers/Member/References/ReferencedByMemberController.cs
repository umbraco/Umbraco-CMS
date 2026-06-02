using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member.References;

/// <summary>
/// Controller responsible for handling API requests related to entities that reference a specific member.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByMemberController : MemberControllerBase
{
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;
    private readonly IMemberReferenceService _memberReferenceService;

    // TODO (V19): Remove the unnecessary parameters provided to the constructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByMemberController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">An implementation of <see cref="ITrackedReferencesService"/> used to manage tracked references.</param>
    /// <param name="relationTypePresentationFactory">An implementation of <see cref="IRelationTypePresentationFactory"/> used to create relation type presentations.</param>
    /// <param name="memberReferenceService">Service for retrieving paged references to a member.</param>
    [ActivatorUtilitiesConstructor]
    public ReferencedByMemberController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IMemberReferenceService memberReferenceService)
    {
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _memberReferenceService = memberReferenceService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByMemberController"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ReferencedByMemberController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
        : this(
            trackedReferencesService,
            relationTypePresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IMemberReferenceService>())
    {
    }

    /// <summary>
    /// Retrieves a paged list of references to the member identified by the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member for which to retrieve references.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="PagedViewModel{IReferenceResponseModel}"/> with references to the specified member.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ReferencedBy2</c> instead. Scheduled for removal in Umbraco 19, when <c>ReferencedBy2</c> will be renamed back to <c>ReferencedBy</c>.
    /// </remarks>
    [Obsolete("Use the ReferencedBy2 action method instead. Scheduled for removal in Umbraco 19, when ReferencedBy2 will be renamed back to ReferencedBy.")]
    [NonAction]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result = await _memberReferenceService.GetPagedReferencesAsync(id, skip, take);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = result.Result.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(result.Result.Items),
        };

        return pagedViewModel;
    }

    /// <summary>
    ///     Retrieves a paginated list of items that reference the specified member, allowing you to see where the member is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media, etc., and for the delete and unpublish operations of single items.
    ///     This method essentially finds parent items in relations.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member for which to find referencing items.</param>
    /// <param name="skip">The number of items to skip when paginating results.</param>
    /// <param name="take">The maximum number of items to return in the paginated result.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged list of references to the specified member.</returns>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a collection of items that reference members.")]
    [EndpointDescription("Gets a paginated collection of items that reference the members identified by the provided Ids.")]
    public async Task<IActionResult> ReferencedBy2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result = await _memberReferenceService.GetPagedReferencesAsync(id, skip, take);

        if (result.Success is false)
        {
            return GetReferencesOperationStatusResult(result.Status);
        }

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = result.Result.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(result.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
