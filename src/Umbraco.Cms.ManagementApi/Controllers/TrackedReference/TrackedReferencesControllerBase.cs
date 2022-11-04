using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReference;

[ApiController]
[VersionedApiBackOfficeRoute("tracked-reference")]
[ApiExplorerSettings(GroupName = "Tracked Reference")]
[ApiVersion("1.0")]
public abstract class TrackedReferenceControllerBase : ManagementApiControllerBase
{
}
