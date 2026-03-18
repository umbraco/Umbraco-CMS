using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

/// <summary>
/// Serves as the base controller for operations related to static file items in the management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileItemControllerBase : ManagementApiControllerBase
{
}
