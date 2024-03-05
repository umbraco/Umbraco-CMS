using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Filter;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/{Constants.UdiEntityType.Member}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Member))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForMemberTree)]
public abstract class MemberFilterControllerBase : ManagementApiControllerBase
{
    protected IActionResult MemberTypeNotFound()
        => OperationStatusResult(ContentEditingOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The requested member type could not be found")
                .Build()));
}
