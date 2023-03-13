using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

public class ItemsDocumentTreeController : DocumentTreeControllerBase
{
    public ItemsDocumentTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, dataTypeService, publicAccessService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentTreeItemResponseModel>>> Items([FromQuery(Name = "key")] Guid[] keys, Guid? dataTypeKey = null, string? culture = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeKey);
        RenderForClientCulture(culture);
        return await GetItems(keys);
    }
}
