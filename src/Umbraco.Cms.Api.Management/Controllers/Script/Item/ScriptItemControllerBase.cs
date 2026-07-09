using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

/// <summary>
/// Serves as the base controller for managing script items via the Umbraco CMS Management API.
/// Provides common functionality for derived script item controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
public class ScriptItemControllerBase : ManagementApiControllerBase
{
}
