using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

public class ByRelationTypeKeyRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationPresentationFactory _relationPresentationFactory;

    public ByRelationTypeKeyRelationController(IRelationService relationService, IRelationPresentationFactory relationPresentationFactory)
    {
        _relationService = relationService;
        _relationPresentationFactory = relationPresentationFactory;
    }

    [HttpGet("type/{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByRelationTypeKey(Guid key, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<IRelation>, RelationOperationStatus> relationsAttempt = await _relationService.GetPagedByRelationTypeKey(key, skip, take);

        if (relationsAttempt.Success is false)
        {
            return await Task.FromResult(RelationOperationStatusResult(relationsAttempt.Status));
        }

        IEnumerable<RelationViewModel> mappedRelations = relationsAttempt.Result.Items.Select(_relationPresentationFactory.Create);

        return await Task.FromResult(Ok(new PagedViewModel<RelationViewModel>
        {
            Total = relationsAttempt.Result.Total,
            Items = mappedRelations,
        }));

    }
}
