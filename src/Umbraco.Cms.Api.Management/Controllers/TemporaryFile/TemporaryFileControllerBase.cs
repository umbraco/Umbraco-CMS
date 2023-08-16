using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[ApiController]
[VersionedApiBackOfficeRoute("temporaryfile")]
[ApiExplorerSettings(GroupName = "Temporary File")]
public abstract class TemporaryFileControllerBase : ManagementApiControllerBase
{
    protected IActionResult TemporaryFileStatusResult(TemporaryFileOperationStatus operationStatus) =>
        operationStatus switch
        {
            TemporaryFileOperationStatus.FileExtensionNotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("File extension not allowed")
                .WithDetail("The file extension is not allowed.")
                .Build()),

            TemporaryFileOperationStatus.KeyAlreadyUsed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Key already used")
                .WithDetail("The specified key is already used.")
                .Build()),

            TemporaryFileOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The temporary file was not found")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown temporary file operation status.")
                .Build()),
        };


    protected IActionResult TemporaryFileNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The temporary file could not be found")
        .Build());
}
