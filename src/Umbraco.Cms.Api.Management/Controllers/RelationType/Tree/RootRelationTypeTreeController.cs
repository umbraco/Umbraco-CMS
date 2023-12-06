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
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemResponseModel>>> Root(int skip = 0, int take = 100)
    {
        PagedModel<IRelationType> pagedRelationTypes = await _relationService.GetPagedRelationTypesAsync(skip, take);

        PagedViewModel<EntityTreeItemResponseModel> pagedResult = PagedViewModel(
            MapTreeItemViewModels(null, pagedRelationTypes.Items.ToArray()),
            pagedRelationTypes.Total);

        return Ok(pagedResult);
    }
}
