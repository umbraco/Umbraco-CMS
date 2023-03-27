using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.RelationType.Tree;

public class RootRelationTypeTreeController : RelationTypeTreeControllerBase
{
    private readonly IRelationService _relationService;

    public RootRelationTypeTreeController(IEntityService entityService, IRelationService relationService)
        : base(entityService) =>
        _relationService = relationService;

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemViewModel>>> Root(int skip = 0, int take = 100)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        // pagination is not supported (yet) by relation service, so we do it in memory for now
        // - chances are we won't have many relation types, so it won't be an actual issue
        IRelationType[] allRelationTypes = _relationService.GetAllRelationTypes().ToArray();

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(
            null,
            allRelationTypes
                .OrderBy(relationType => relationType.Name)
                .Skip((int)(pageNumber * pageSize))
                .Take(pageSize)
                .ToArray());

        PagedViewModel<EntityTreeItemViewModel> result = PagedViewModel(viewModels, allRelationTypes.Length);
        return await Task.FromResult(Ok(result));
    }
}
