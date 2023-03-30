using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Items;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
public class DatatypeEntityControllerBase : ManagementApiControllerBase
{
}
