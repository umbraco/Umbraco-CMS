using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Install;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/install")]
[OpenApiTag("Install")]
public abstract class InstallControllerBase : Controller
{
}
