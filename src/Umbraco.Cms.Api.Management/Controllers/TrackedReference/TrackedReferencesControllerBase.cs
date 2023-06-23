using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.TrackedReference;

[ApiController]
[VersionedApiBackOfficeRoute("tracked-reference")]
[ApiExplorerSettings(GroupName = "Tracked Reference")]
public abstract class TrackedReferenceControllerBase : ManagementApiControllerBase
{
}
