using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMembers)]
public class MemberGroupControllerBase : ManagementApiControllerBase
{
    protected IActionResult MemberGroupOperationStatusResult(MemberGroupOperationStatus status) =>
        status switch
        {
            MemberGroupOperationStatus.Success => Ok(),
            MemberGroupOperationStatus.NotFound => MemberGroupNotFound(),
            MemberGroupOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the member group operation.")
                .Build()),
            MemberGroupOperationStatus.CannotHaveEmptyName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Name was empty or null")
                .WithDetail("The provided member group name cannot be null or empty.")
                .Build()),
            MemberGroupOperationStatus.DuplicateName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate name")
                .WithDetail("Another group with the same name already exists.")
                .Build()),
            MemberGroupOperationStatus.DuplicateKey => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate key")
                .WithDetail("Another group with the same key already exists.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown member group operation status.")
                .Build()),
        };

    protected IActionResult MemberGroupNotFound() => OperationStatusResult(MemberGroupOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The requested member group could not be found")
                .Build()));
}
