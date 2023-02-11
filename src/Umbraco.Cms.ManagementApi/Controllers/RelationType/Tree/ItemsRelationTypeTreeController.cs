using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.RelationType.Tree;

public class ItemsRelationTypeTreeController : RelationTypeTreeControllerBase
{
    private readonly IRelationService _relationService;

    public ItemsRelationTypeTreeController(IEntityService entityService, IRelationService relationService)
        : base(entityService) =>
        _relationService = relationService;

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FolderTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        // relation service does not allow fetching a collection of relation types by their ids; instead it relies
        // heavily on caching, which means this is as fast as it gets - even if it looks less than performant
        IRelationType[] relationTypes = _relationService
            .GetAllRelationTypes()
            .Where(relationType => keys.Contains(relationType.Key)).ToArray();

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(null, relationTypes);

        return await Task.FromResult(Ok(viewModels));
    }
}
