using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[VersionedApiBackOfficeRoute("published-cache")]
[ApiExplorerSettings(GroupName = "Published Cache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
