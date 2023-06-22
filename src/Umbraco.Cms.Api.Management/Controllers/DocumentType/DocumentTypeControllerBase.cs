using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DocumentType)]
[ApiExplorerSettings(GroupName = "Document Type")]
public abstract class DocumentTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status) =>
        status switch
        {
            ContentTypeOperationStatus.Success => Ok(),
            ContentTypeOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not Found")
                .WithDetail("The content type was not found")
                .Build()),
            ContentTypeOperationStatus.DuplicateAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate alias")
                .WithDetail("The alias is already in use")
                .Build()),
            ContentTypeOperationStatus.InvalidAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid alias")
                .WithDetail("The alias is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidDataType => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid data type")
                .WithDetail("The data type is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidPropertyTypeAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid property type alias")
                .WithDetail("The property type alias is invalid")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content type operation status"),
        };
}
