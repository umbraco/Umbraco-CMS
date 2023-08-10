using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DocumentType)]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class DocumentTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => ContentTypeOperationStatusResult(status, "content");

    internal static IActionResult ContentTypeOperationStatusResult(ContentTypeOperationStatus status, string type) =>
        status switch
        {
            ContentTypeOperationStatus.Success => new OkResult(),
            ContentTypeOperationStatus.NotFound => new NotFoundObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Not Found")
                .WithDetail($"The specified {type} type was not found")
                .Build()),
            ContentTypeOperationStatus.DuplicateAlias => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Duplicate alias")
                .WithDetail($"The specified {type} type alias is already in use")
                .Build()),
            ContentTypeOperationStatus.InvalidAlias => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Invalid alias")
                .WithDetail($"The specified {type} type alias is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidPropertyTypeAlias => new BadRequestObjectResult(
                new ProblemDetailsBuilder()
                    .WithTitle("Invalid property type alias")
                    .WithDetail("One or more property type aliases are invalid")
                    .Build()),
            ContentTypeOperationStatus.InvalidContainerName => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Invalid container name")
                .WithDetail("One or more container names are invalid")
                .Build()),
            ContentTypeOperationStatus.MissingContainer => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Missing container")
                .WithDetail("One or more containers or properties are listed as parents to containers that are not defined.")
                .Build()),
            ContentTypeOperationStatus.DuplicateContainer => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Duplicate container")
                .WithDetail("One or more containers (or container keys) are defined multiple times.")
                .Build()),
            ContentTypeOperationStatus.DataTypeNotFound => new NotFoundObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Data Type not found")
                .WithDetail("One or more of the specified data types were not found")
                .Build()),
            ContentTypeOperationStatus.InvalidInheritance => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Invalid inheritance")
                .WithDetail($"The specified {type} type inheritance is invalid")
                .Build()),
            ContentTypeOperationStatus.InvalidComposition => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Invalid composition")
                .WithDetail($"The specified {type} type composition is invalid")),
            ContentTypeOperationStatus.InvalidParent => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Invalid parent")
                .WithDetail(
                    "The specified parent is invalid, or cannot be used in combination with the specified composition/inheritance")),
            ContentTypeOperationStatus.DuplicatePropertyTypeAlias => new BadRequestObjectResult(
                new ProblemDetailsBuilder()
                    .WithTitle("Duplicate property type alias")
                    .WithDetail("One or more property type aliases are already in use, all property type aliases must be unique.")
                    .Build()),
            _ => new ObjectResult("Unknown content type operation status") { StatusCode = StatusCodes.Status500InternalServerError },
        };
}
