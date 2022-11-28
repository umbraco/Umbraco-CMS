using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Profiling;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("profiling")]
[ApiExplorerSettings(GroupName = "Profiling")]
public class ProfilingControllerBase : ManagementApiControllerBase
{
}
