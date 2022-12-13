using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiController]
[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
[ApiVersion("1.0")]
public class RedirectUrlManagementBaseController : ManagementApiControllerBase
{

}
