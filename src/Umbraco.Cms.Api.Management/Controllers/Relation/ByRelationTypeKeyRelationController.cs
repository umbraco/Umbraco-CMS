using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

/// <summary>
/// Controller for managing relations filtered by relation type key.
/// </summary>
[ApiVersion("1.0")]
public class ByRelationTypeKeyRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationPresentationFactory _relationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByRelationTypeKeyRelationController"/> class, which manages relations filtered by relation type key.
    /// </summary>
    /// <param name="relationService">Service used to manage and query relations.</param>
    /// <param name="relationPresentationFactory">Factory for creating relation presentation models.</param>
    public ByRelationTypeKeyRelationController(IRelationService relationService, IRelationPresentationFactory relationPresentationFactory)
    {
        _relationService = relationService;
        _relationPresentationFactory = relationPresentationFactory;
    }

/// <summary>
/// Retrieves a paged list of relations filtered by the specified relation type key.
/// </summary>
/// <remarks>
/// Use case: On a relation type page, you can view all relations that have been created for a particular relation type.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="id">The unique identifier (key) of the relation type to filter relations by.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set.</param>
/// <param name="take">The maximum number of items to return in the result set.</param>
/// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged list of <see cref="RelationResponseModel"/> objects.</returns>
    [HttpGet("type/{id:guid}", Name = "GetRelationByRelationTypeId")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedViewModel<ProblemDetails>), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets relations by relation type.")]
    [EndpointDescription("Gets a collection of relations filtered by the specified relation type key.")]
    public async Task<IActionResult> ByRelationTypeKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IRelation>, RelationOperationStatus> relationsAttempt = await _relationService.GetPagedByRelationTypeKeyAsync(id, skip, take);

        if (relationsAttempt.Success is false)
        {
            return RelationOperationStatusResult(relationsAttempt.Status);
        }

        IEnumerable<RelationResponseModel> mappedRelations = relationsAttempt.Result.Items.Select(_relationPresentationFactory.Create);

        return Ok(new PagedViewModel<RelationResponseModel>
        {
            Total = relationsAttempt.Result.Total,
            Items = mappedRelations,
        });
    }
}
