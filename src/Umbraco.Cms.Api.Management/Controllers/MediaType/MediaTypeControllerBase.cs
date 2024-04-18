using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MediaType)]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public abstract class MediaTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeOperationStatusResult(status, "media");

    protected IActionResult StructureOperationStatusResult(ContentTypeStructureOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeStructureOperationStatusResult(status, "media");

    protected static IActionResult MediaTypeImportOperationStatusResult(MediaTypeImportOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            MediaTypeImportOperationStatus.TemporaryFileNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("Temporary file not found")
                .Build()),
            MediaTypeImportOperationStatus.TemporaryFileConversionFailure => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Failed to convert the specified file")
                .WithDetail($"The import failed due to not being able to convert the file into proper xml")
                .Build()),
            MediaTypeImportOperationStatus.MediaTypeExists => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Failed to import because media type exits")
                .WithDetail($"The import failed because the media type that was being imported already exits and the {nameof(ImportMediaTypeRequestModel.OverWriteExisting)} flag was disabled")
                .Build()),
            _ => new ObjectResult("Unknown content type import operation status") { StatusCode = StatusCodes.Status500InternalServerError },
        });
}
