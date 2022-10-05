using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.Tree;

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

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys, Guid? dataTypeKey = null, string? culture = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeKey);
        RenderForClientCulture(culture);
        return await GetItems(keys);
    }
}
