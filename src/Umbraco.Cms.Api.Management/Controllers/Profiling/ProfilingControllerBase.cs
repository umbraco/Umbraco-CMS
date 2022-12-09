using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("profiling")]
[ApiExplorerSettings(GroupName = "Profiling")]
public class ProfilingControllerBase : ManagementApiControllerBase
{
}
