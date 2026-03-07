using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Culture;

    /// <summary>
    /// Serves as the base controller for API endpoints that manage culture-related operations in the Umbraco CMS.
    /// </summary>
[VersionedApiBackOfficeRoute("culture")]
[ApiExplorerSettings(GroupName = "Culture")]
public abstract class CultureControllerBase : ManagementApiControllerBase
{
}
