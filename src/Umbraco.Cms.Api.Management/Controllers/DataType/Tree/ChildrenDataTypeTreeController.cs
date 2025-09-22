using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

[ApiVersion("1.0")]
public class ChildrenDataTypeTreeController : DataTypeTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDataTypeTreeController(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService, dataTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ChildrenDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
        : base(entityService, flagProviders, dataTypeService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DataTypeTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
