using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.NewsDashboard;

[VersionedApiBackOfficeRoute("news-dashboard")]
[ApiExplorerSettings(GroupName = "News Dashboard")]
public abstract class NewsDashboardControllerBase : ManagementApiControllerBase
{
}
