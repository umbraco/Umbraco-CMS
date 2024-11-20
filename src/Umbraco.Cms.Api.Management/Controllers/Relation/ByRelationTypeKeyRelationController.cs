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

[ApiVersion("1.0")]
public class ByRelationTypeKeyRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationPresentationFactory _relationPresentationFactory;

    public ByRelationTypeKeyRelationController(IRelationService relationService, IRelationPresentationFactory relationPresentationFactory)
    {
        _relationService = relationService;
        _relationPresentationFactory = relationPresentationFactory;
    }

    /// <summary>
    /// Gets a paged list of relations by the unique relation key.
    /// </summary>
    /// <remarks>
    /// Use case: On a relation type page you can see all created relations of this type.
    /// </remarks>
    [HttpGet("type/{id:guid}", Name = "GetRelationByRelationTypeId")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedViewModel<ProblemDetails>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRelationTypeKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IRelation>, RelationOperationStatus> relationsAttempt = await _relationService.GetPagedByRelationTypeKeyAsync(id, skip, take);

        if (relationsAttempt.Success is false)
        {
            return await Task.FromResult(RelationOperationStatusResult(relationsAttempt.Status));
        }

        IEnumerable<RelationResponseModel> mappedRelations = relationsAttempt.Result.Items.Select(_relationPresentationFactory.Create);

        return await Task.FromResult(Ok(new PagedViewModel<RelationResponseModel>
        {
            Total = relationsAttempt.Result.Total,
            Items = mappedRelations,
        }));

    }
}
