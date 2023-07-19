using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiController]
[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessContent)]
public class RedirectUrlManagementControllerBase : ManagementApiControllerBase
{

}
