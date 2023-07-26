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
        // TODO: rephrase all these error messages
        // TODO: ensure the switch case covers all operation status values
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
            ContentTypeOperationStatus.InvalidPropertyTypeAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid property type alias")
                .WithDetail("The property type alias is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidContainerName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid container name")
                .WithDetail("The container name is invalid")
                .Build()),
            ContentTypeOperationStatus.DataTypeNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Data Type not found")
                .WithDetail("The requested data type was not found")
                .Build()),
            ContentTypeOperationStatus.InvalidInheritance => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid inheritance")
                .WithDetail("The specified inheritance is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidComposition => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid composition")
                .WithDetail("The specified composition is invalid")),
            ContentTypeOperationStatus.InvalidParent => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid parent")
                .WithDetail("The specified parent is invalid, or cannot be used in combination with the specified composition/inheritance")),
            ContentTypeOperationStatus.DuplicatePropertyTypeAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate property type alias")
                .WithDetail("The property type alias is already in use, all property type aliases must be unique.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content type operation status"),
        };
}
