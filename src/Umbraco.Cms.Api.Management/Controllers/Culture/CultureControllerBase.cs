using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Culture;

[VersionedApiBackOfficeRoute("culture")]
[ApiExplorerSettings(GroupName = "Culture")]
public abstract class CultureControllerBase : ManagementApiControllerBase
{
}
