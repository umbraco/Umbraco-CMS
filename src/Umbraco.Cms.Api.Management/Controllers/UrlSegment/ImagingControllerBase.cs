using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.UrlSegment;

[VersionedApiBackOfficeRoute("imaging")]
[ApiExplorerSettings(GroupName = "Imaging")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ImagingControllerBase : ManagementApiControllerBase
{
}
