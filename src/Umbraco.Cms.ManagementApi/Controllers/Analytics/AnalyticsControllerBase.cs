using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/analytics")]
[OpenApiTag("Analytics")]
public class AnalyticsControllerBase : Controller
{
}
