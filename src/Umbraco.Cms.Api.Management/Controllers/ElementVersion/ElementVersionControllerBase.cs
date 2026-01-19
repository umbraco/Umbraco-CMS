using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentVersion;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Element}-version")]
[ApiExplorerSettings(GroupName = $"{nameof(Constants.UdiEntityType.Element)} Version")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public abstract class ElementVersionControllerBase : ManagementApiControllerBase
{
    protected IActionResult MapFailure(ContentVersionOperationStatus status)
        => DocumentVersionControllerBase.MapFailure(status);
}
