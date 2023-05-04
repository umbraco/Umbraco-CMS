using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[ApiVersion("1.0")]
public class ChildrenDocumentTreeController : DocumentTreeControllerBase
{
    public ChildrenDocumentTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, dataTypeService, publicAccessService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTreeItemResponseModel>>> Children(Guid parentId, int skip = 0, int take = 100, Guid? dataTypeId = null, string? culture = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        RenderForClientCulture(culture);
        return await GetChildren(parentId, skip, take);
    }
}
