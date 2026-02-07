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
public class RootDataTypeTreeController : DataTypeTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootDataTypeTreeController(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService, dataTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RootDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
    : base(entityService, flagProviders, dataTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of data type items from the root of the data type tree.")]
    [EndpointDescription("Gets a paged collection of data type items from the root of the data type tree. The collection can be optionally filtered to return only folder, or folders and data types. The result returns the data type tree items' properties.")]
    public async Task<ActionResult<PagedViewModel<DataTypeTreeItemResponseModel>>> Root(CancellationToken cancellationToken, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
