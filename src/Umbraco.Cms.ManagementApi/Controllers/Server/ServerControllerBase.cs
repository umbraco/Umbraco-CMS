using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

[ApiController]
[VersionedApiBackOfficeRoute("server")]
[ApiExplorerSettings(GroupName = "Server")]
public abstract class ServerControllerBase : ManagementApiControllerBase
{

}
