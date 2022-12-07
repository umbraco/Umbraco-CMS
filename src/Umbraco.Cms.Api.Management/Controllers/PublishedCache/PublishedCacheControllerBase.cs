using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("published-cache")]
[ApiExplorerSettings(GroupName = "Published Cache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
