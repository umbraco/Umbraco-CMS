using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;

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

    public RelationDisplay? GetById(int id) =>
        _umbracoMapper.Map<IRelation, RelationDisplay>(_relationService.GetById(id));
}
