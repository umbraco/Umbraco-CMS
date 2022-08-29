using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

[ApiController]
[ApiVersion("1.0")]
[BackOfficeRoute("api/v{version:apiVersion}/server")]
[OpenApiTag("Server")]
public class ServerController : Controller
{

}
