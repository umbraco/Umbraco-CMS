using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

public class ByChildRelationController : RelationControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationViewModelFactory _relationViewModelFactory;

    public ByChildRelationController(
        IRelationService relationService,
        IRelationViewModelFactory relationViewModelFactory)
    {
        _relationService = relationService;
        _relationViewModelFactory = relationViewModelFactory;
    }

    [HttpGet("childRelations/{childId:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<RelationViewModel>> ByChild(int childId, int skip, int take, string? relationTypeAlias = "")
    {
        IRelation[] relations = _relationService.GetByChildId(childId).ToArray();
        RelationViewModel[] result = Array.Empty<RelationViewModel>();

        if (relations.Any())
        {
            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                result = _relationViewModelFactory.CreateMultiple(relations.Where(x =>
                    x.RelationType.Alias.InvariantEquals(relationTypeAlias))).ToArray();
            }
            else
            {
                result = _relationViewModelFactory.CreateMultiple(relations).ToArray();
            }
        }

        return await Task.FromResult(new PagedViewModel<RelationViewModel>
        {
            Total = result.Length,
            Items = result.Skip(skip).Take(take),
        });
    }
}
