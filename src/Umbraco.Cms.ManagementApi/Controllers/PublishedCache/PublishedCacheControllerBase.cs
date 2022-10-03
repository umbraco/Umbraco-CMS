using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

[ApiVersion("1.0")]
[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/published-cache")]
[OpenApiTag("PublishedCache")]
public class PublishedCacheControllerBase : ManagementApiControllerBase
{
}
