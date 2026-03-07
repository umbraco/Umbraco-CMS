using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

    /// <summary>
    /// Serves as the base controller for API endpoints related to content preview functionality in the management area.
    /// </summary>
[VersionedApiBackOfficeRoute("preview")]
[ApiExplorerSettings(GroupName = "Preview")]
public class PreviewControllerBase : ManagementApiControllerBase
{
}
