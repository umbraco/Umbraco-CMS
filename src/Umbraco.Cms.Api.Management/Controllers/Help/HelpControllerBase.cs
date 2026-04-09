using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Help;

/// <summary>
/// Serves as the base controller for help-related API endpoints in Umbraco CMS management, providing shared functionality for derived help controllers.
/// </summary>
[Obsolete("This is no longer used. Scheduled for removal in Umbraco 19.")]
[VersionedApiBackOfficeRoute("help")]
[ApiExplorerSettings(GroupName = "Help")]
public abstract class HelpControllerBase : ManagementApiControllerBase
{
}
