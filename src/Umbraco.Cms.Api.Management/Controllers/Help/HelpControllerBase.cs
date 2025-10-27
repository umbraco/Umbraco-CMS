using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Help;

[Obsolete("This is no longer used and will be removed in v19")]
[VersionedApiBackOfficeRoute("help")]
[ApiExplorerSettings(GroupName = "Help")]
public abstract class HelpControllerBase : ManagementApiControllerBase
{
}
