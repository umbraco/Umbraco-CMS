using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Help;

[Obsolete("This is no longer used. Scheduled for removal in Umbraco 19.")]
[VersionedApiBackOfficeRoute("help")]
[ApiExplorerSettings(GroupName = "Help")]
public abstract class HelpControllerBase : ManagementApiControllerBase
{
}
