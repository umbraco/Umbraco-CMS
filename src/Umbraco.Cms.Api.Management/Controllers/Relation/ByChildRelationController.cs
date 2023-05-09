using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
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

    [HttpGet("child-relation/{childId:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationResponseModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<RelationResponseModel>> ByChild(int childId, int skip, int take, string? relationTypeAlias = "")
    {
        IRelation[] relations = _relationService.GetByChildId(childId).ToArray();
        RelationResponseModel[] result = Array.Empty<RelationResponseModel>();

        if (relations.Any())
        {
            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                result = _relationPresentationFactory.CreateMultiple(relations.Where(x =>
                    x.RelationType.Alias.InvariantEquals(relationTypeAlias))).ToArray();
            }
            else
            {
                result = _relationPresentationFactory.CreateMultiple(relations).ToArray();
            }
        }

        return await Task.FromResult(new PagedViewModel<RelationResponseModel>
        {
            Total = result.Length,
            Items = result.Skip(skip).Take(take),
        });
    }
}
