using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

[ApiController]
[VersionedApiBackOfficeRoute("server")]
[OpenApiTag("Server")]
public abstract class ServerControllerBase : ManagementApiControllerBase
{

}
