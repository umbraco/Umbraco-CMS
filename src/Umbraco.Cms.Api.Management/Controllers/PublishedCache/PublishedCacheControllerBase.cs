using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

    /// <summary>
    /// Serves as the base controller for API endpoints that manage the published cache in Umbraco CMS.
    /// </summary>
[VersionedApiBackOfficeRoute("published-cache")]
[ApiExplorerSettings(GroupName = "Published Cache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
