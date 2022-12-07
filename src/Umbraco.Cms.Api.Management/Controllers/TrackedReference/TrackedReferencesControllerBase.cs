using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.TrackedReference;

[ApiController]
[VersionedApiBackOfficeRoute("tracked-reference")]
[ApiExplorerSettings(GroupName = "Tracked Reference")]
[ApiVersion("1.0")]
public abstract class TrackedReferenceControllerBase : ManagementApiControllerBase
{
}
