using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Import;

/// <summary>
/// Serves as the base controller for import operations in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute("import")]
[ApiExplorerSettings(GroupName = "Import")]
public abstract class ImportControllerBase : ManagementApiControllerBase
{
    protected static IActionResult TemporaryFileXmlImportOperationStatusResult(TemporaryFileXmlImportOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            TemporaryFileXmlImportOperationStatus.TemporaryFileNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("Temporary file not found")
                .Build()),
            TemporaryFileXmlImportOperationStatus.UndeterminedEntityType => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Unable to determine entity type")
                .Build()),
            _ => new ObjectResult("Unknown temporary file import operation status") { StatusCode = StatusCodes.Status500InternalServerError },
        });
}
