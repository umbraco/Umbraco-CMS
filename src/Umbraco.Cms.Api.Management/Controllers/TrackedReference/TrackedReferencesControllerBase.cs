using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.TrackedReference;

[ApiController]
[VersionedApiBackOfficeRoute("tracked-reference")]
[ApiExplorerSettings(GroupName = "Tracked Reference")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessContentOrMedia)]
public abstract class TrackedReferenceControllerBase : ManagementApiControllerBase
{
}
