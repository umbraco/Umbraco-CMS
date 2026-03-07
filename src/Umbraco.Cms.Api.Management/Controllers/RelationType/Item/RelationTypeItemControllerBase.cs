using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

    /// <summary>
    /// Serves as the base controller for operations related to relation type items in the Umbraco CMS Management API.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
public class RelationTypeItemControllerBase : ManagementApiControllerBase
{
}
