using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.RelationType.Tree;

public class RelationTypeTreeItemsController : RelationTypeTreeControllerBase
{
    public RelationTypeTreeItemsController(IEntityService entityService, IRelationService relationService)
        : base(entityService, relationService)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FolderTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        // TODO: either make EntityService support relation types, or make RelationService able to query multiple relation types
        // - for now this workaround works somewhat, as there likely isn't a whole lot of relation types defined.
        IRelationType[] relationTypes = RelationService
            .GetAllRelationTypes()
            .Where(relationType => keys.Contains(relationType.Key)).ToArray();

        TreeItemViewModel[] viewModels = MapTreeItemViewModels(null, relationTypes);

        PagedResult<TreeItemViewModel> result = PagedResult(viewModels, 0, viewModels.Length, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }
}
