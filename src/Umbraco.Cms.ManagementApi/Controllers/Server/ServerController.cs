using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

[ApiController]
[ApiVersion("1.0")]
[BackOfficeRoute("api/v{version:apiVersion}/server")]
public class ServerController : Controller
{

}
