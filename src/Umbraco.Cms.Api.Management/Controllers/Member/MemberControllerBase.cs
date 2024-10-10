using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Member)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Member))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMembers)]
public class MemberControllerBase : ContentControllerBase
{
    protected IActionResult MemberNotFound() => OperationStatusResult(MemberEditingOperationStatus.MemberNotFound, MemberNotFound);

    protected IActionResult MemberEditingStatusResult(MemberEditingStatus status)
        => status.MemberEditingOperationStatus is not MemberEditingOperationStatus.Success
            ? MemberEditingOperationStatusResult(status.MemberEditingOperationStatus)
            : status.ContentEditingOperationStatus is not ContentEditingOperationStatus.Success
                ? ContentEditingOperationStatusResult(status.ContentEditingOperationStatus)
                : throw new ArgumentException("Please handle success status explicitly in the controllers", nameof(status));

    protected IActionResult MemberEditingOperationStatusResult(MemberEditingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            MemberEditingOperationStatus.MemberNotFound => MemberNotFound(problemDetailsBuilder),
            MemberEditingOperationStatus.MemberTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested member type could not be found")
                .Build()),
            MemberEditingOperationStatus.UnlockFailed => BadRequest(problemDetailsBuilder
                .WithTitle("Could not unlock the member")
                .WithDetail("Please refer to the logs for additional information.")
                .Build()),
            MemberEditingOperationStatus.DisableTwoFactorFailed => BadRequest(problemDetailsBuilder
                .WithTitle("Could not disable 2FA for the member")
                .WithDetail("Please refer to the logs for additional information.")
                .Build()),
            MemberEditingOperationStatus.RoleAssignmentFailed => BadRequest(problemDetailsBuilder
                .WithTitle("Could not update role assignments for the member")
                .WithDetail("Please refer to the logs for additional information.")
                .Build()),
            MemberEditingOperationStatus.PasswordChangeFailed => BadRequest(problemDetailsBuilder
                .WithTitle("Could not change the password")
                .WithDetail(
                    "This is likely because the password did not meet the complexity requirements. The logs might hold additional information.")
                .Build()),
            MemberEditingOperationStatus.InvalidPassword => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid password supplied")
                .WithDetail("The password did not meet the complexity requirements.")
                .Build()),
            MemberEditingOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid name supplied")
                .Build()),
            MemberEditingOperationStatus.InvalidUsername => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid username supplied")
                .Build()),
            MemberEditingOperationStatus.InvalidEmail => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid email supplied")
                .Build()),
            MemberEditingOperationStatus.DuplicateUsername => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate username detected")
                .WithDetail("The supplied username is already in use by another member.")
                .Build()),
            MemberEditingOperationStatus.DuplicateEmail => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate email detected")
                .WithDetail("The supplied email is already in use by another member.")
                .Build()),
            MemberEditingOperationStatus.CancelledByNotificationHandler => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled")
                .Build()),
            MemberEditingOperationStatus.Unknown => StatusCode(
                StatusCodes.Status500InternalServerError,
                problemDetailsBuilder
                    .WithTitle("Unknown error. Please see the log for more details.")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown member operation status.")
                .Build())
        });

    protected IActionResult MemberEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<MemberValueModel, MemberVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, MemberValueModel, MemberVariantRequestModel>(status, requestModel, validationResult);

    private IActionResult MemberNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The requested member could not be found")
        .Build());
}
