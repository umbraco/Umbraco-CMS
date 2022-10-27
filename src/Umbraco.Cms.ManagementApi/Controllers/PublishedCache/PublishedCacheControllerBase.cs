using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("published-cache")]
[OpenApiTag("Published Cache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
