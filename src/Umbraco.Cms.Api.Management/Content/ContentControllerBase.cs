using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Content;

public class ContentControllerBase : ManagementApiControllerBase
{
    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentEditingOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the content operation.")
                .Build()),
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested content could not be found")
                .Build()),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            ContentEditingOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The content could not be found")
                .Build()),
            ContentEditingOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The targeted content parent could not be found")
                .Build()),
            ContentEditingOperationStatus.ParentInvalid => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid parent")
                .WithDetail("The targeted parent was not valid for the operation.")
                .Build()),
            ContentEditingOperationStatus.NotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("Operation not permitted")
                .WithDetail(
                    "The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                .Build()),
            ContentEditingOperationStatus.TemplateNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The template could not be found")
                .Build()),
            ContentEditingOperationStatus.TemplateNotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("Template not allowed")
                .WithDetail("The selected template was not allowed for the operation.")
                .Build()),
            ContentEditingOperationStatus.PropertyTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("One or more property types could not be found")
                .Build()),
            ContentEditingOperationStatus.InTrash => BadRequest(problemDetailsBuilder
                .WithTitle("Content is in the recycle bin")
                .WithDetail("Could not perform the operation because the targeted content was in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.NotInTrash => BadRequest(problemDetailsBuilder
                .WithTitle("Content is not in the recycle bin")
                .WithDetail("The attempted operation requires the targeted content to be in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.SortingInvalid => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid sorting options")
                .WithDetail("The supplied sorting operations were invalid. Additional details can be found in the log.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(StatusCodes.Status500InternalServerError,
                problemDetailsBuilder
                    .WithTitle("Unknown error. Please see the log for more details.")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });

    protected IActionResult ContentEditingOperationStatusResult<TContentModelBase, TValueModel, TVariantModel>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        IEnumerable<PropertyValidationError> validationErrors)
        where TContentModelBase : ContentModelBase<TValueModel, TVariantModel>
        where TValueModel : ValueModelBase
        where TVariantModel : VariantModelBase
    {
        if (status is not ContentEditingOperationStatus.PropertyValidationError)
        {
            return ContentEditingOperationStatusResult(status);
        }

        var errors = new SortedDictionary<string, string[]>();
        foreach (PropertyValidationError validationError in validationErrors)
        {
            TValueModel? requestValue = requestModel.Values.FirstOrDefault(value =>
                value.Alias == validationError.Alias
                && value.Culture == validationError.Culture
                && value.Segment == validationError.Segment);
            if (requestValue is null)
            {
                // TODO: throw up, log, anything goes - ThisShouldNotHappen(tm)
                continue;
            }

            var index = requestModel.Values.IndexOf(requestValue);
            var key = $"$.{nameof(ContentModelBase<TValueModel, TVariantModel>.Values).ToFirstLowerInvariant()}[{index}].{nameof(ValueModelBase.Value).ToFirstLowerInvariant()}{validationError.JsonPath}";
            errors.Add(key, validationError.ErrorMessages);
        }

        return OperationStatusResult(status, problemDetailsBuilder
            => BadRequest(problemDetailsBuilder
                .WithTitle("Validation failed")
                .WithDetail("One or more properties did not pass validation")
                .WithOperationStatus(status)
                .WithPropertyValidationErrors(errors)
                .Build()));
    }

    protected IActionResult ContentPublishingOperationStatusResult(ContentPublishingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentPublishingOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested content could not be found")
                .Build()),
            ContentPublishingOperationStatus.CancelledByEvent => BadRequest(problemDetailsBuilder
                .WithTitle("Publish cancelled by event")
                .WithDetail("The publish operation was cancelled by an event.")
                .Build()),
            ContentPublishingOperationStatus.ContentInvalid => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid content")
                .WithDetail("The specified content had an invalid configuration.")
                .Build()),
            ContentPublishingOperationStatus.NothingToPublish => BadRequest(problemDetailsBuilder
                .WithTitle("Nothing to publish")
                .WithDetail("None of the specified cultures needed publishing.")
                .Build()),
            ContentPublishingOperationStatus.MandatoryCultureMissing => BadRequest(problemDetailsBuilder
                .WithTitle("Mandatory culture missing")
                .WithDetail("Must include all mandatory cultures when publishing.")
                .Build()),
            ContentPublishingOperationStatus.HasExpired => BadRequest(problemDetailsBuilder
                .WithTitle("Content expired")
                .WithDetail("Could not publish the content because it was expired.")
                .Build()),
            ContentPublishingOperationStatus.CultureHasExpired => BadRequest(problemDetailsBuilder
                .WithTitle("Content culture expired")
                .WithDetail("Could not publish the content because some of the specified cultures were expired.")
                .Build()),
            ContentPublishingOperationStatus.AwaitingRelease => BadRequest(problemDetailsBuilder
                .WithTitle("Content awaiting release")
                .WithDetail("Could not publish the content because it was awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.CultureAwaitingRelease => BadRequest(problemDetailsBuilder
                .WithTitle("Content culture awaiting release")
                .WithDetail(
                    "Could not publish the content because some of the specified cultures were awaiting release.")
                .Build()),
            ContentPublishingOperationStatus.InTrash => BadRequest(problemDetailsBuilder
                .WithTitle("Content in the recycle bin")
                .WithDetail("Could not publish the content because it was in the recycle bin.")
                .Build()),
            ContentPublishingOperationStatus.PathNotPublished => BadRequest(problemDetailsBuilder
                .WithTitle("Parent not published")
                .WithDetail("Could not publish the content because its parent was not published.")
                .Build()),
            ContentPublishingOperationStatus.ConcurrencyViolation => BadRequest(problemDetailsBuilder
                .WithTitle("Concurrency violation detected")
                .WithDetail("An attempt was made to publish a version older than the latest version.")
                .Build()),
            ContentPublishingOperationStatus.UnsavedChanges => BadRequest(problemDetailsBuilder
                .WithTitle("Unsaved changes")
                .WithDetail(
                    "Could not publish the content because it had unsaved changes. Make sure to save all changes before attempting a publish.")
                .Build()),
            ContentPublishingOperationStatus.Failed => BadRequest(problemDetailsBuilder
                .WithTitle("Publish or unpublish failed")
                .WithDetail(
                    "An unspecified error occurred while (un)publishing. Please check the logs for additional information.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        });

    protected IActionResult ContentCreatingOperationStatusResult(ContentCreatingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentCreatingOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The content type could not be found")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });

    protected IActionResult PublicAccessOperationStatusResult(PublicAccessOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            PublicAccessOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The content could not be found")
                .Build()),
            PublicAccessOperationStatus.ErrorNodeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The error page could not be found")
                .Build()),
            PublicAccessOperationStatus.LoginNodeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The login page could not be found")
                .Build()),
            PublicAccessOperationStatus.NoAllowedEntities => BadRequest(problemDetailsBuilder
                .WithTitle("No allowed entities given")
                .WithDetail("Both MemberGroups and Members were empty, thus no entities can be allowed.")
                .Build()),
            PublicAccessOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Request cancelled by notification")
                .WithDetail("The request to save a public access entry was cancelled by a notification handler.")
                .Build()),
            PublicAccessOperationStatus.AmbiguousRule => BadRequest(problemDetailsBuilder
                .WithTitle("Ambiguous Rule")
                .WithDetail("The specified rule is ambiguous, because both member groups and member names were given.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });
}
