using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[ApiVersion("1.0")]
public class ByChildRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationPresentationFactory _relationPresentationFactory;

    public ByChildRelationController(
        IRelationService relationService,
        IRelationPresentationFactory relationPresentationFactory)
    {
        _relationService = relationService;
        _relationPresentationFactory = relationPresentationFactory;
    }

    /// <summary>
    /// Gets a paged list of relations by the unique relation child keys.
    /// </summary>
    /// <remarks>
    /// Use case: When you wanna restore a deleted item, this is used to find the old location
    /// </remarks>
    [HttpGet("child-relation/{childId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByChild(Guid childId, int skip = 0, int take = 100, string? relationTypeAlias = "")
    {
        PagedModel<IRelation> relationsAttempt = await _relationService.GetPagedByChildKeyAsync(childId, skip, take, relationTypeAlias);

        IEnumerable<RelationResponseModel> mappedRelations = relationsAttempt.Items.Select(_relationPresentationFactory.Create);

        return await Task.FromResult(Ok(new PagedViewModel<RelationResponseModel>
        {
            Total = relationsAttempt.Total,
            Items = mappedRelations,
        }));
    }
}
