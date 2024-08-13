using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType;

[ApiVersion("1.0")]
public class AllRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllRelationTypeController(
        IRelationService relationService,
        IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationTypeResponseModel>>> Get(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<IRelationType> allRelationTypes = await _relationService.GetPagedRelationTypesAsync(skip, take);

        var pagedResult = new PagedViewModel<RelationTypeResponseModel>
        {
            Total = allRelationTypes.Total,
            Items = _umbracoMapper.MapEnumerable<IRelationType, RelationTypeResponseModel>(allRelationTypes.Items.Skip(skip).Take(take)),
        };

        return Ok(pagedResult);
    }
}
