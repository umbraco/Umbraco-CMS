using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Culture;

[ApiController]
[VersionedApiBackOfficeRoute("culture")]
[ApiExplorerSettings(GroupName = "Culture")]
[ApiVersion("1.0")]
public abstract class CultureControllerBase : ManagementApiControllerBase
{
}
