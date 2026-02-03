using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

[ApiVersion("1.0")]
public class SearchDataTypeTreeController : DataTypeTreeControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IIdKeyMap _idKeyMap;

    public SearchDataTypeTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IDataTypeService dataTypeService,
        IEntitySearchService entitySearchService,
        IUmbracoMapper mapper,
        IIdKeyMap idKeyMap)
        : base(entityService, flagProviders, dataTypeService)
    {
        _entitySearchService = entitySearchService;
        _idKeyMap = idKeyMap;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DataTypeTreeItemResponseModel>>> Search(CancellationToken cancellationToken, string? query, int skip = 0, int take = 100)
        => await SearchTreeEntities(_entitySearchService, _idKeyMap, query, skip, take);
}
