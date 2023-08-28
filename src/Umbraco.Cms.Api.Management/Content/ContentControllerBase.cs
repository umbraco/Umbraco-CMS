using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Content;

public class ContentControllerBase : ManagementApiControllerBase
{
    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status) =>
        status switch
        {
            ContentEditingOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the content operation.")
                .Build()),
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .Build()),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            ContentEditingOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The content could not be found")
                    .Build()),
            ContentEditingOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The targeted content parent could not be found")
                    .Build()),
            ContentEditingOperationStatus.ParentInvalid => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid parent")
                .WithDetail("The targeted parent was not valid for the operation.")
                .Build()),
            ContentEditingOperationStatus.NotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Operation not permitted")
                .WithDetail("The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                .Build()),
            ContentEditingOperationStatus.TemplateNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The template could not be found")
                    .Build()),
            ContentEditingOperationStatus.TemplateNotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Template not allowed")
                .WithDetail("The selected template was not allowed for the operation.")
                .Build()),
            ContentEditingOperationStatus.PropertyTypeNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("One or more property types could not be found")
                    .Build()),
            ContentEditingOperationStatus.InTrash => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is in the recycle bin")
                .WithDetail("Could not perform the operation because the targeted content was in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.NotInTrash => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is not in the recycle bin")
                .WithDetail("The attempted operation requires the targeted content to be in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.SortingInvalid => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid sorting options")
                .WithDetail("The supplied sorting operations were invalid. Additional details can be found in the log.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown error. Please see the log for more details.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown content operation status.")
                .Build()),
        };

    protected IActionResult ContentCreatingOperationStatusResult(ContentCreatingOperationStatus status) =>
        status switch
        {
            ContentCreatingOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The content type could not be found")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown content operation status.")
                .Build()),
        };

    protected IActionResult PublicAccessOperationStatusResult(PublicAccessOperationStatus status) =>
        status switch
        {
            PublicAccessOperationStatus.ContentNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The content could not be found")
                    .Build()),
            PublicAccessOperationStatus.ErrorNodeNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The error page could not be found")
                    .Build()),
            PublicAccessOperationStatus.LoginNodeNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The login page could not be found")
                    .Build()),
            PublicAccessOperationStatus.NoAllowedEntities => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("No allowed entities given")
                .WithDetail("Both MemberGroups and Members were empty, thus no entities can be allowed.")
                .Build()),
            PublicAccessOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Request cancelled by notification")
                .WithDetail("The request to save a public access entry was cancelled by a notification handler.")
                .Build()),
            PublicAccessOperationStatus.AmbiguousRule => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Ambiguous Rule")
                .WithDetail("The specified rule is ambiguous, because both member groups and member names were given.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown content operation status.")
                .Build()),
        };
}
