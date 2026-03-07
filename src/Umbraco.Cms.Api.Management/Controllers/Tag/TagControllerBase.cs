using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Tag;

    /// <summary>
    /// Serves as the base controller for tag management endpoints in the Umbraco CMS Management API.
    /// Provides common functionality for derived tag controllers.
    /// </summary>
[VersionedApiBackOfficeRoute("tag")]
[ApiExplorerSettings(GroupName = "Tag")]
public class TagControllerBase : ManagementApiControllerBase
{
}
