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

    protected IActionResult ContentPublishingOperationStatusResult(ContentPublishingOperationStatus status) =>
        status switch
        {
            ContentPublishingOperationStatus.ContentNotFound => NotFound("The content type could not be found"),
            ContentPublishingOperationStatus.CancelledByEvent => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Publish cancelled by event")
                .WithDetail("The publish operation was cancelled by an event.")
                .Build()),
            ContentPublishingOperationStatus.ContentInvalid => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid content")
                .WithDetail("The specified content had an invalid configuration.")
                .Build()),
            ContentPublishingOperationStatus.NothingToPublish => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Nothing to publish")
                .WithDetail("None of the specified cultures needed publishing.")
                .Build()),
            ContentPublishingOperationStatus.MandatoryCultureMissing => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Mandatory culture missing")
                .WithDetail("Must include all mandatory cultures when publishing.")
                .Build()),
            ContentPublishingOperationStatus.HasExpired => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content expired")
                .WithDetail("Could not publish the content because it was expired.")
                .Build()),
            ContentPublishingOperationStatus.CultureHasExpired => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content culture expired")
                .WithDetail("Could not publish the content because some of the specified cultures were expired.")
                .Build()),
            ContentPublishingOperationStatus.AwaitingRelease => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content awaiting release")
                .WithDetail("Could not publish the content because it was awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.CultureAwaitingRelease => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content culture awaiting release")
                .WithDetail("Could not publish the content because some of the specified cultures were awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.InTrash => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content in the recycle bin")
                .WithDetail("Could not publish the content because it was in the recycle bin.")
                .Build()),
            ContentPublishingOperationStatus.PathNotPublished => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Parent not published")
                .WithDetail("Could not publish the content because its parent was not published.")
                .Build()),
            ContentPublishingOperationStatus.ConcurrencyViolation => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Concurrency violation detected")
                .WithDetail("An attempt was made to publish a version older than the latest version.")
                .Build()),
            ContentPublishingOperationStatus.UnsavedChanges => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Unsaved changes")
                .WithDetail("Could not publish the content because it had unsaved changes. Make sure to save all changes before attempting a publish.")
                .Build()),
            ContentPublishingOperationStatus.Failed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Publish or unpublish failed")
                .WithDetail("An unspecified error occurred while (un)publishing. Please check the logs for additional information.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
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
