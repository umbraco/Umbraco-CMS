using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class RedirectUrlManagementControllerBase : ManagementApiControllerBase
{
}
