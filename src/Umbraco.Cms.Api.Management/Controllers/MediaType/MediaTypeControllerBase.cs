using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MediaType)]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
public abstract class MediaTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeOperationStatusResult(status, "media");

    protected IActionResult StructureOperationStatusResult(ContentTypeStructureOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeStructureOperationStatusResult(status, "media");

    protected IActionResult MediaTypeImportOperationStatusResult(MediaTypeImportOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            MediaTypeImportOperationStatus.TemporaryFileNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Temporary file not found")
                .Build()),
            MediaTypeImportOperationStatus.TemporaryFileConversionFailure => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to convert the specified file")
                .WithDetail("The import failed due to not being able to convert the file into proper xml.")
                .Build()),
            MediaTypeImportOperationStatus.MediaTypeExists => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to import because media type exits")
                .WithDetail("The import failed because the media type that was being imported already exits.")
                .Build()),
            MediaTypeImportOperationStatus.TypeMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Type Mismatch")
                .WithDetail("The import failed because the file contained an entity that is not a media type.")
                .Build()),
            MediaTypeImportOperationStatus.IdMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Id")
                .WithDetail("The import failed because the id of the media type you are trying to update did not match the id in the file.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown media type import operation status.")
        });
}
