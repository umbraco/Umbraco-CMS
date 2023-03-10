using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[ApiController]
[VersionedApiBackOfficeRoute("temporaryfile")]
[ApiExplorerSettings(GroupName = "Temporary File")]
[ApiVersion("1.0")]
public abstract class TemporaryFileControllerBase : ManagementApiControllerBase
{
    protected IActionResult TemporaryFileStatusResult(TemporaryFileStatus status) =>
        status switch
        {
            TemporaryFileStatus.FileExtensionNotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("File extension not allowed")
                .WithDetail("The file extension is not allowed.")
                .Build()),

            TemporaryFileStatus.KeyAlreadyUsed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Key already used")
                .WithDetail("The specified key is already used.")
                .Build()),

            TemporaryFileStatus.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown temporary file operation status")
        };


    protected new IActionResult NotFound() => NotFound("The temporary file could not be found");
}
