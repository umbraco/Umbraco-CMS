using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Help;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/help")]
[OpenApiTag("Help")]
[ApiVersion("1.0")]
public abstract class HelpControllerBase : ManagementApiControllerBase
{
}
