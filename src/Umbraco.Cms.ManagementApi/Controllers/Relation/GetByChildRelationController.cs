using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

public class GetByChildRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetByChildRelationController(IRelationService relationService, IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("/getByChild/{childId:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationViewModel), StatusCodes.Status200OK)]
    public IEnumerable<RelationViewModel> GetByChild(int childId, string relationTypeAlias = "")
    {
        IRelation[] relations = _relationService.GetByChildId(childId).ToArray();

        if (relations.Any() == false)
        {
            return Enumerable.Empty<RelationViewModel>();
        }

        if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
        {
            return _umbracoMapper.MapEnumerable<IRelation, RelationViewModel>(relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias))).WhereNotNull();
        }

        return _umbracoMapper.MapEnumerable<IRelation, RelationViewModel>(relations).WhereNotNull();
    }
}
