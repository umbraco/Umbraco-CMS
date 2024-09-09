using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

[ApiVersion("1.0")]
public class ItemRelationTypeItemController : RelationTypeItemControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _mapper;

    public ItemRelationTypeItemController(IRelationService relationService, IUmbracoMapper mapper)
    {
        _relationService = relationService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<RelationTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<RelationTypeItemResponseModel>());
        }

        // relation service does not allow fetching a collection of relation types by their ids; instead it relies
        // heavily on caching, which means this is as fast as it gets - even if it looks less than performant
        IRelationType[] relationTypes = _relationService
            .GetAllRelationTypes()
            .Where(relationType => ids.Contains(relationType.Key)).ToArray();

        List<RelationTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IRelationType, RelationTypeItemResponseModel>(relationTypes);

        return await Task.FromResult(Ok(responseModels));
    }
}
