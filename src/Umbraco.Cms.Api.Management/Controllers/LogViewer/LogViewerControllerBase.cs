using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiController]
[VersionedApiBackOfficeRoute("log-viewer")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
[ApiVersion("1.0")]
public abstract class LogViewerControllerBase : ManagementApiControllerBase
{
}
