using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

[ApiVersion("1.0")]
public class RootMediaTreeController : MediaTreeControllerBase
{
    public RootMediaTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, dataTypeService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MediaTreeItemResponseModel>>> Root(int skip = 0, int take = 100, Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetRoot(skip, take);
    }
}
