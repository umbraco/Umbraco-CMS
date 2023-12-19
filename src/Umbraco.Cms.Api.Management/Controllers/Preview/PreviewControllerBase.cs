using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiController]
[VersionedApiBackOfficeRoute("preview")]
[ApiExplorerSettings(GroupName = "Preview")]
public class PreviewControllerBase : ManagementApiControllerBase
{
}
