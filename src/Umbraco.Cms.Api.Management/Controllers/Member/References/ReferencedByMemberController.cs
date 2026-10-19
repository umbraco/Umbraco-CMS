using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByMemberController"/> class.
    /// </summary>
    /// <param name="relationTypePresentationFactory">An implementation of <see cref="IRelationTypePresentationFactory"/> used to create relation type presentations.</param>
    /// <param name="memberReferenceService">Service for retrieving paged references to a member.</param>
    public ReferencedByMemberController(
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IMemberReferenceService memberReferenceService)
    {
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _memberReferenceService = memberReferenceService;
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
    public async Task<IActionResult> ReferencedBy(
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
