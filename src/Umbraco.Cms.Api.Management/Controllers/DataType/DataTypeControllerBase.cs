using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DataType)]
[ApiExplorerSettings(GroupName = "Data Type")]
[ApiVersion("1.0")]
public abstract class DataTypeControllerBase : ManagementApiControllerBase
{
}
