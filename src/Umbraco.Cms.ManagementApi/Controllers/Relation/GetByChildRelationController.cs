using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

public class GetByChildRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IPagedViewModelFactory _pagedViewModelFactory;

    public GetByChildRelationController(IRelationService relationService, IUmbracoMapper umbracoMapper, IPagedViewModelFactory pagedViewModelFactory)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
        _pagedViewModelFactory = pagedViewModelFactory;
    }

    [HttpGet("childRelations/{childId:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationViewModel), StatusCodes.Status200OK)]
    public PagedViewModel<RelationViewModel> GetByChild(int childId, int skip, int take, string? relationTypeAlias = "")
    {
        IRelation[] relations = _relationService.GetByChildId(childId).ToArray();
        IEnumerable<RelationViewModel> result = Enumerable.Empty<RelationViewModel>();

        if (relations.Any())
        {
            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                result = _umbracoMapper
                    .MapEnumerable<IRelation, RelationViewModel>(relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
                    .WhereNotNull();
            }
            else
            {
                result = _umbracoMapper.MapEnumerable<IRelation, RelationViewModel>(relations).WhereNotNull();
            }
        }

        return _pagedViewModelFactory.Create(result, skip, take);
    }
}
