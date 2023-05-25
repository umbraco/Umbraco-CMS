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
public class RootDocumentTreeController : DocumentTreeControllerBase
{
    public RootDocumentTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IContentTypeService contentTypeService)
        : base(entityService, userStartNodeEntitiesService, dataTypeService, publicAccessService, appCaches, backofficeSecurityAccessor, contentTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTreeItemResponseModel>>> Root(int skip = 0, int take = 100, Guid? dataTypeId = null, string? culture = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        RenderForClientCulture(culture);
        return await GetRoot(skip, take);
    }
}
