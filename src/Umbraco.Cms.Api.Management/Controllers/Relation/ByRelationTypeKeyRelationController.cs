using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

public class ByRelationTypeKeyRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationViewModelFactory _relationViewModelFactory;

    public ByRelationTypeKeyRelationController(IRelationService relationService, IRelationViewModelFactory relationViewModelFactory)
    {
        _relationService = relationService;
        _relationViewModelFactory = relationViewModelFactory;
    }

    [HttpGet("type/{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ByRelationTypeKey(Guid key, int skip = 0, int take = 100)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out var problemDetails) is false)
        {
            return BadRequest(problemDetails);
        }

        PagedModel<IRelation> relations = await _relationService.GetPagedByRelationTypeId(key, pageNumber, pageSize);
        IEnumerable<RelationViewModel> mappedRelations = relations.Items.Select(_relationViewModelFactory.Create);

        return await Task.FromResult(Ok(new PagedViewModel<RelationViewModel>
        {
            Total = relations.Total,
            Items = mappedRelations,
        }));
    }
}
