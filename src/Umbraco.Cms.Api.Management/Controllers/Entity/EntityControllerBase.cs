using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Entity;

[ApiController]
[VersionedApiBackOfficeRoute("entity")]
[ApiExplorerSettings(GroupName = "entity")]
[ApiVersion("1.0")]
public class EntityControllerBase : ManagementApiControllerBase
{
}
