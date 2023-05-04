using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiController]
[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
public class RedirectUrlManagementControllerBase : ManagementApiControllerBase
{

}
