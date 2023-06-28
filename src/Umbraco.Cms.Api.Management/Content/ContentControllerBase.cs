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
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound("The content type could not be found"),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            ContentEditingOperationStatus.NotFound => NotFound("The content could not be found"),
            ContentEditingOperationStatus.ParentNotFound => NotFound("The targeted content parent could not be found"),
            ContentEditingOperationStatus.ParentInvalid => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid parent")
                .WithDetail("The targeted parent was not valid for the operation.")
                .Build()),
            ContentEditingOperationStatus.NotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Operation not permitted")
                .WithDetail("The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                .Build()),
            ContentEditingOperationStatus.TemplateNotFound => NotFound("The template could not be found"),
            ContentEditingOperationStatus.TemplateNotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Template not allowed")
                .WithDetail("The selected template was not allowed for the operation.")
                .Build()),
            ContentEditingOperationStatus.PropertyTypeNotFound => NotFound("One or more property types could not be found"),
            ContentEditingOperationStatus.InTrash => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is in the recycle bin")
                .WithDetail("Could not perform the operation because the targeted content was in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(StatusCodes.Status500InternalServerError, "Unknown error. Please see the log for more details."),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status.")
        };

    protected IActionResult ContentCreatingOperationStatusResult(ContentCreatingOperationStatus status) =>
        status switch
        {
            ContentCreatingOperationStatus.ContentTypeNotFound => NotFound("The content type could not be found"),
            ContentCreatingOperationStatus.NotFound => NotFound("The content type could not be found"),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        };
}
