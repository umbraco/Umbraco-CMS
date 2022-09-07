using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

[ApiVersion("1.0")]
public class GetRelationController : RelationControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IRelationService _relationService;

    public GetRelationController(IUmbracoMapper umbracoMapper, IRelationService relationService)
    {
        _umbracoMapper = umbracoMapper;
        _relationService = relationService;
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationViewModel), StatusCodes.Status200OK)]
    public RelationViewModel Get(int id) =>
        _umbracoMapper.Map<IRelation, RelationViewModel>(_relationService.GetById(id))!;
}
