using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[VersionedApiBackOfficeRoute("temporary-file")]
[ApiExplorerSettings(GroupName = "Temporary File")]
public abstract class TemporaryFileControllerBase : ManagementApiControllerBase
{
    protected IActionResult TemporaryFileStatusResult(TemporaryFileOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            TemporaryFileOperationStatus.FileExtensionNotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("File extension not allowed")
                .WithDetail("The file extension is not allowed.")
                .Build()),
            TemporaryFileOperationStatus.InvalidFileName => BadRequest(problemDetailsBuilder
                .WithTitle("The provided file name is not valid")
                .Build()),
            TemporaryFileOperationStatus.KeyAlreadyUsed => BadRequest(problemDetailsBuilder
                .WithTitle("Key already used")
                .WithDetail("The specified key is already used.")
                .Build()),
            TemporaryFileOperationStatus.NotFound => TemporaryFileNotFound(problemDetailsBuilder),
            TemporaryFileOperationStatus.UploadBlocked => NotFound(problemDetailsBuilder
                .WithTitle("The temporary file was blocked by a validator")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown temporary file operation status.")
                .Build()),
        });

    protected IActionResult TemporaryFileNotFound() => OperationStatusResult(TemporaryFileOperationStatus.NotFound, TemporaryFileNotFound);

    private IActionResult TemporaryFileNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The temporary file could not be found")
        .Build());
}
