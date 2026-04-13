using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.NewsDashboard;

/// <summary>
/// Serves as the base controller for implementing News Dashboard-related endpoints in the management API.
/// Provides shared functionality for derived News Dashboard controllers.
/// </summary>
[VersionedApiBackOfficeRoute("news-dashboard")]
[ApiExplorerSettings(GroupName = "News Dashboard")]
public abstract class NewsDashboardControllerBase : ManagementApiControllerBase
{
}
