using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("published-cache")]
[ApiExplorerSettings(GroupName = "Published Cache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
