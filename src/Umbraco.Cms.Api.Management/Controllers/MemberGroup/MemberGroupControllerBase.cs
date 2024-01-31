using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMembers)]
public class MemberGroupControllerBase : ManagementApiControllerBase
{
    protected IActionResult MemberGroupOperationStatusResult(MemberGroupOperationStatus status) =>
        status switch
        {
            MemberGroupOperationStatus.Success => Ok(),
            MemberGroupOperationStatus.CannotHaveEmptyName => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Name was empty or null")
                .WithDetail("The provided member group name cannot be null or empty.")
                .Build()),
            MemberGroupOperationStatus.DuplicateName => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Duplicate name")
                .WithDetail("Another group with the same name already exists.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown member group operation status.")
                .Build()),
        };
}
