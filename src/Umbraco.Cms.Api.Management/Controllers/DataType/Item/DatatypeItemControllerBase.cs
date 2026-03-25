using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Item;

/// <summary>
/// Serves as the base controller for operations related to data type items in the management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
public class DatatypeItemControllerBase : ManagementApiControllerBase
{
}
