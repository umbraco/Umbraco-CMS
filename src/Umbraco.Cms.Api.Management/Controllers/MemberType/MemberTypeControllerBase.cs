using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MemberType)]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
public abstract class MemberTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeOperationStatusResult(status, "member");

    protected IActionResult StructureOperationStatusResult(ContentTypeStructureOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeStructureOperationStatusResult(status, "member");

    protected IActionResult MemberTypeImportOperationStatusResult(MemberTypeImportOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            MemberTypeImportOperationStatus.TemporaryFileNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Temporary file not found")
                .Build()),
            MemberTypeImportOperationStatus.TemporaryFileConversionFailure => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to convert the specified file")
                .WithDetail("The import failed due to not being able to convert the file into proper xml.")
                .Build()),
            MemberTypeImportOperationStatus.MemberTypeExists => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to import because member type exists")
                .WithDetail("The import failed because the member type that was being imported already exits.")
                .Build()),
            MemberTypeImportOperationStatus.TypeMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Type Mismatch")
                .WithDetail("The import failed because the file contained an entity that is not a member type.")
                .Build()),
            MemberTypeImportOperationStatus.IdMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Id")
                .WithDetail("The import failed because the id of the member type you are trying to update did not match the id in the file.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown member type import operation status."),
        });
}
