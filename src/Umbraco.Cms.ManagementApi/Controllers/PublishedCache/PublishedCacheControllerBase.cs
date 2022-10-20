using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("published-cache")]
[OpenApiTag("PublishedCache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
