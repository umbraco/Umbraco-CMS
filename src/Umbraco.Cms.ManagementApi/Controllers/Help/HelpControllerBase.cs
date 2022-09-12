using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Help;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/help")]
[OpenApiTag("Help")]
public abstract class HelpControllerBase : Controller
{
}
