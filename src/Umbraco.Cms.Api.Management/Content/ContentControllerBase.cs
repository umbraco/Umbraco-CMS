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

    protected IActionResult ContentPublishingOperationStatusResult(ContentPublishingOperationStatus status) =>
        status switch
        {
            ContentPublishingOperationStatus.ContentNotFound => NotFound("The content type could not be found"),
            ContentPublishingOperationStatus.FailedCancelledByEvent => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Publish cancelled by event")
                .WithDetail("The publish operation was cancelled by an event.")
                .Build()),
            ContentPublishingOperationStatus.FailedContentInvalid => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The content was invalid")
                .WithDetail("The content with the given key, had an invalid configuration.")
                .Build()),
            ContentPublishingOperationStatus.FailedNothingToPublish => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Nothing to publish")
                .WithDetail("None of the cultures given needed publishing.")
                .Build()),
            ContentPublishingOperationStatus.FailedMandatoryCultureMissing => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Mandatory culture missing")
                .WithDetail("Cannot publish some cultures, without all the mandatory cultures.")
                .Build()),
            ContentPublishingOperationStatus.FailedHasExpired => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content has expired")
                .WithDetail("Cannot publish as the content status was expired.")
                .Build()),
            ContentPublishingOperationStatus.FailedCultureHasExpired => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content has expired")
                .WithDetail("Cannot publish as one of the given cultures content status was expired.")
                .Build()),
            ContentPublishingOperationStatus.FailedAwaitingRelease => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is awaiting release")
                .WithDetail("Cannot publish as the content status was awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.FailedCultureAwaitingRelease => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is awaiting release")
                .WithDetail("Cannot publish as the content status was awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.FailedIsTrashed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is trashed")
                .WithDetail("Cannot publish as the content status was trashed")
                .Build()),
            ContentPublishingOperationStatus.FailedPathNotPublished => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Parent is not published")
                .WithDetail("Cannot publish as the contents parent is allowed.")
                .Build()),

            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        };

    protected IActionResult ContentCreatingOperationStatusResult(ContentCreatingOperationStatus status) =>
        status switch
        {
            ContentCreatingOperationStatus.NotFound => NotFound("The content type could not be found"),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        };

    protected IActionResult PublicAccessOperationStatusResult(PublicAccessOperationStatus status) =>
        status switch
        {
            PublicAccessOperationStatus.ContentNotFound => NotFound("The content could not be found"),
            PublicAccessOperationStatus.ErrorNodeNotFound => NotFound("The error page could not be found"),
            PublicAccessOperationStatus.LoginNodeNotFound => NotFound("The login page could not be found"),
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
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        };
}
