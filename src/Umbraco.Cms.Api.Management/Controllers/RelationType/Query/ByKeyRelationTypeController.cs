using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

public class ByKeyRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _mapper;

    public ByKeyRelationTypeController(IRelationService relationService, IUmbracoMapper mapper)
    {
        _relationService = relationService;
        _mapper = mapper;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationTypeViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByKey(Guid key)
    {
        IRelationType? relationType = _relationService.GetRelationTypeById(key);
        RelationTypeViewModel mappedRelationType = _mapper.Map<RelationTypeViewModel>(relationType)!;

        return await Task.FromResult(Ok(mappedRelationType));
    }
}
