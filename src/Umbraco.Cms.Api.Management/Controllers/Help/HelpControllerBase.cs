using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Help;

[ApiController]
[VersionedApiBackOfficeRoute("help")]
[ApiExplorerSettings(GroupName = "Help")]
public abstract class HelpControllerBase : ManagementApiControllerBase
{
}
