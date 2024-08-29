using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DocumentType)]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public abstract class DocumentTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => ContentTypeOperationStatusResult(status, "document");

    protected IActionResult StructureOperationStatusResult(ContentTypeStructureOperationStatus status)
        => ContentTypeStructureOperationStatusResult(status, "document");

    internal static IActionResult ContentTypeOperationStatusResult(ContentTypeOperationStatus status, string type) =>
        status is ContentTypeOperationStatus.Success
            ? new OkResult()
            : OperationStatusResult(status, problemDetailsBuilder => status switch
            {
                ContentTypeOperationStatus.NotFound => new NotFoundObjectResult(problemDetailsBuilder
                    .WithTitle("Not Found")
                    .WithDetail($"The specified {type} type was not found")
                    .Build()),
                ContentTypeOperationStatus.DuplicateAlias => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Duplicate alias")
                    .WithDetail($"The specified {type} type alias is already in use")
                    .Build()),
                ContentTypeOperationStatus.InvalidAlias => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid alias")
                    .WithDetail($"The specified {type} type alias is invalid")
                    .Build()),
                ContentTypeOperationStatus.InvalidPropertyTypeAlias => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid property type alias")
                    .WithDetail("One or more property type aliases are invalid")
                    .Build()),
                ContentTypeOperationStatus.PropertyTypeAliasCannotEqualContentTypeAlias => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid property type alias")
                    .WithDetail("The property type alias cannot be the same as the content type alias")
                    .Build()),
                ContentTypeOperationStatus.InvalidContainerName => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid container name")
                    .WithDetail("One or more container names are invalid")
                    .Build()),
                ContentTypeOperationStatus.InvalidContainerType => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid container type")
                    .WithDetail("One or more container types are invalid")
                    .Build()),
                ContentTypeOperationStatus.MissingContainer => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Missing container")
                    .WithDetail("One or more containers or properties are listed as parents to containers that are not defined.")
                    .Build()),
                ContentTypeOperationStatus.DuplicateContainer => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Duplicate container")
                    .WithDetail("One or more containers (or container keys) are defined multiple times.")
                    .Build()),
                ContentTypeOperationStatus.DataTypeNotFound => new NotFoundObjectResult(problemDetailsBuilder
                    .WithTitle("Data Type not found")
                    .WithDetail("One or more of the specified data types were not found")
                    .Build()),
                ContentTypeOperationStatus.InvalidInheritance => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid inheritance")
                    .WithDetail($"The specified {type} type inheritance is invalid")
                    .Build()),
                ContentTypeOperationStatus.InvalidComposition => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid composition")
                    .WithDetail($"The specified {type} type composition is invalid")
                    .Build()),
                ContentTypeOperationStatus.InvalidParent => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid parent")
                    .WithDetail("The specified parent is invalid, or cannot be used in combination with the specified composition/inheritance")
                    .Build()),
                ContentTypeOperationStatus.DuplicatePropertyTypeAlias => new BadRequestObjectResult(
                    problemDetailsBuilder
                        .WithTitle("Duplicate property type alias")
                        .WithDetail("One or more property type aliases are already in use, all property type aliases must be unique.")
                        .Build()),
                ContentTypeOperationStatus.NotAllowed => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Operation not permitted")
                    .WithDetail("The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                    .Build()),
                ContentTypeOperationStatus.CancelledByNotification => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Cancelled by notification")
                    .WithDetail("The attempted operation was cancelled by a notification.")
                    .Build()),
                ContentTypeOperationStatus.NameCannotBeEmpty => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Name cannot be empty")
                    .WithDetail("The name of the content type cannot be empty")
                    .Build()),
                ContentTypeOperationStatus.NameTooLong => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Name was too long")
                    .WithDetail("Name cannot be more than 255 characters in length.")
                    .Build()),
                ContentTypeOperationStatus.InvalidElementFlagDocumentHasContent => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid IsElement flag")
                    .WithDetail("Cannot change to element type because content has already been created with this document type.")
                    .Build()),
                ContentTypeOperationStatus.InvalidElementFlagElementIsUsedInPropertyEditorConfiguration => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid IsElement flag")
                    .WithDetail("Cannot change to document type because this element type is used in the configuration of a data type.")
                    .Build()),
                ContentTypeOperationStatus.InvalidElementFlagComparedToParent => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Invalid IsElement flag")
                    .WithDetail("Can not create a documentType with inheritance composition where the parent and the new type's IsElement flag are different.")
                    .Build()),
                _ => new ObjectResult("Unknown content type operation status") { StatusCode = StatusCodes.Status500InternalServerError },
            });

    public static IActionResult ContentTypeStructureOperationStatusResult(ContentTypeStructureOperationStatus status, string type) =>
        status is ContentTypeStructureOperationStatus.Success
            ? new OkResult()
            : OperationStatusResult(status, problemDetailsBuilder => status switch
            {
                ContentTypeStructureOperationStatus.CancelledByNotification => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Cancelled by notification")
                    .WithDetail($"A notification handler prevented the {type} type operation")
                    .Build()),
                ContentTypeStructureOperationStatus.ContainerNotFound => new NotFoundObjectResult(problemDetailsBuilder
                    .WithTitle("Container not found")
                    .WithDetail("The specified container was not found")
                    .Build()),
                ContentTypeStructureOperationStatus.NotAllowedByPath => new BadRequestObjectResult(problemDetailsBuilder
                    .WithTitle("Not allowed by path")
                    .WithDetail($"The {type} type operation cannot be performed due to not allowed path (i.e. a child of itself)")
                    .Build()),
                ContentTypeStructureOperationStatus.NotFound => new NotFoundObjectResult(problemDetailsBuilder
                    .WithTitle("Not Found")
                    .WithDetail($"The specified {type} type was not found")
                    .Build()),
                _ => new ObjectResult("Unknown content type structure operation status") { StatusCode = StatusCodes.Status500InternalServerError }
            });

    protected IActionResult ContentTypeImportOperationStatusResult(ContentTypeImportOperationStatus operationStatus) =>
        OperationStatusResult(operationStatus, problemDetailsBuilder => operationStatus switch
        {
            ContentTypeImportOperationStatus.TemporaryFileNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Temporary file not found")
                .Build()),
            ContentTypeImportOperationStatus.TemporaryFileConversionFailure => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to convert the specified file")
                .WithDetail("The import failed due to not being able to convert the file into proper xml")
                .Build()),
            ContentTypeImportOperationStatus.DocumentTypeExists => BadRequest(problemDetailsBuilder
                .WithTitle("Failed to import because document type exits")
                .WithDetail("The import failed because the document type that was being imported already exits")
                .Build()),
            ContentTypeImportOperationStatus.TypeMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Type Mismatch")
                .WithDetail("The import failed because the file contained an entity that is not a content type.")
                .Build()),
            ContentTypeImportOperationStatus.IdMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Id")
                .WithDetail("The import failed because the id of the document type you are trying to update did not match the id in the file.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown document type import operation status.")
        });

    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The specified document type was not found")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content editing operation status")
                .Build()),
        });
}
