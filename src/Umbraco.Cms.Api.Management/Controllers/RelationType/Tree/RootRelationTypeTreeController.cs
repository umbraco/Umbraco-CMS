using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Tree;

[ApiVersion("1.0")]
public class RootRelationTypeTreeController : RelationTypeTreeControllerBase
{
    private readonly IRelationService _relationService;

    public RootRelationTypeTreeController(IEntityService entityService, IRelationService relationService)
        : base(entityService) =>
        _relationService = relationService;

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationTypeTreeItemResponseModel>>> Root(int skip = 0, int take = 100)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        // pagination is not supported (yet) by relation service, so we do it in memory for now
        // - chances are we won't have many relation types, so it won't be an actual issue
        IRelationType[] allRelationTypes = _relationService.GetAllRelationTypes().ToArray();

        RelationTypeTreeItemResponseModel[] viewModels = MapTreeItemViewModels(
            null,
            allRelationTypes
                .OrderBy(relationType => relationType.Name)
                .Skip((int)(pageNumber * pageSize))
                .Take(pageSize)
                .ToArray());

        PagedViewModel<RelationTypeTreeItemResponseModel> result = PagedViewModel(viewModels, allRelationTypes.Length);
        return await Task.FromResult(Ok(result));
    }
}
