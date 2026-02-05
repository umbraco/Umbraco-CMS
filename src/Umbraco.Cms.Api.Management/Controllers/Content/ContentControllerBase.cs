using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Content;

public abstract class ContentControllerBase : ManagementApiControllerBase
{
    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentEditingOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the content operation.")
                .Build()),
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested content type could not be found")
                .Build()),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            ContentEditingOperationStatus.ContentTypeSegmentVarianceMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Content type segment variance mismatch")
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
            ContentEditingOperationStatus.InvalidCulture => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid culture")
                .WithDetail("One or more of the supplied culture codes did not match the configured languages.")
                .Build()),
            ContentEditingOperationStatus.DuplicateKey => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Id")
                .WithDetail("The supplied id is already in use.")
                .Build()),
            ContentEditingOperationStatus.DuplicateName => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate name")
                .WithDetail("The supplied name is already in use for the same content type.")
                .Build()),
            ContentEditingOperationStatus.CannotDeleteWhenReferenced => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot delete a referenced content item")
                .WithDetail("Cannot delete a referenced document, while the setting ContentSettings.DisableDeleteWhenReferenced is enabled.")
                .Build()),
            ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot move a referenced document to the recycle bin")
                .WithDetail("Cannot move a referenced document to the recycle bin, while the setting ContentSettings.DisableUnpublishWhenReferenced is enabled.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(
                StatusCodes.Status500InternalServerError,
                problemDetailsBuilder
                    .WithTitle("Unknown error. Please see the log for more details.")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });

    protected IActionResult GetReferencesOperationStatusResult(GetReferencesOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            GetReferencesOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested content could not be found")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown get references operation status.")
                .Build()),
        });

    protected IActionResult ContentPublishingOperationStatusResult(
        ContentPublishingOperationStatus status,
        IEnumerable<string>? invalidPropertyAliases = null,
        IEnumerable<ContentPublishingBranchItemResult>? failedBranchItems = null)
        => OperationStatusResult(
            status,
            problemDetailsBuilder => status switch
            {
                ContentPublishingOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The requested document could not be found")
                    .Build()),
                ContentPublishingOperationStatus.CancelledByEvent => BadRequest(problemDetailsBuilder
                    .WithTitle("Publish cancelled by event")
                    .WithDetail("The publish operation was cancelled by an event.")
                    .Build()),
                ContentPublishingOperationStatus.ContentInvalid => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid document")
                    .WithDetail("The specified document had an invalid configuration.")
                    .WithExtension("invalidProperties", invalidPropertyAliases ?? Enumerable.Empty<string>())
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
                    .WithTitle("Document expired")
                    .WithDetail("Could not publish the document because it was expired.")
                    .Build()),
                ContentPublishingOperationStatus.CultureHasExpired => BadRequest(problemDetailsBuilder
                    .WithTitle("Document culture expired")
                    .WithDetail("Could not publish the document because some of the specified cultures were expired.")
                    .Build()),
                ContentPublishingOperationStatus.AwaitingRelease => BadRequest(problemDetailsBuilder
                    .WithTitle("Document awaiting release")
                    .WithDetail("Could not publish the document because it was awaiting release.")
                    .Build()),
                ContentPublishingOperationStatus.CultureAwaitingRelease => BadRequest(problemDetailsBuilder
                    .WithTitle("Document culture awaiting release")
                    .WithDetail(
                        "Could not publish the document because some of the specified cultures were awaiting release.")
                    .Build()),
                ContentPublishingOperationStatus.InTrash => BadRequest(problemDetailsBuilder
                    .WithTitle("Document in the recycle bin")
                    .WithDetail("Could not publish the document because it was in the recycle bin.")
                    .Build()),
                ContentPublishingOperationStatus.PathNotPublished => BadRequest(problemDetailsBuilder
                    .WithTitle("Parent not published")
                    .WithDetail("Could not publish the document because its parent was not published.")
                    .Build()),
                ContentPublishingOperationStatus.InvalidCulture => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid cultures specified")
                    .WithDetail("A specified culture is not valid for the operation.")
                    .Build()),
                ContentPublishingOperationStatus.CultureMissing => BadRequest(problemDetailsBuilder
                    .WithTitle("Culture missing")
                    .WithDetail("A culture needs to be specified to execute the operation.")
                    .Build()),
                ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant => BadRequest(problemDetailsBuilder
                    .WithTitle("Cannot publish invariant when variant")
                    .WithDetail("Cannot publish invariant culture when the document varies by culture.")
                    .Build()),
                ContentPublishingOperationStatus.CannotPublishVariantWhenNotVariant => BadRequest(problemDetailsBuilder
                    .WithTitle("Cannot publish variant when not variant.")
                    .WithDetail("Cannot publish a given culture when the document is invariant.")
                    .Build()),
                ContentPublishingOperationStatus.ConcurrencyViolation => BadRequest(problemDetailsBuilder
                    .WithTitle("Concurrency violation detected")
                    .WithDetail("An attempt was made to publish a version older than the latest version.")
                    .Build()),
                ContentPublishingOperationStatus.UnsavedChanges => BadRequest(problemDetailsBuilder
                    .WithTitle("Unsaved changes")
                    .WithDetail(
                        "Could not publish the document because it had unsaved changes. Make sure to save all changes before attempting a publish.")
                    .Build()),
                ContentPublishingOperationStatus.UnpublishTimeNeedsToBeAfterPublishTime => BadRequest(problemDetailsBuilder
                    .WithTitle("Unpublish time needs to be after the publish time")
                    .WithDetail(
                        "Cannot handle an unpublish time that is not after the specified publish time.")
                    .Build()),
                ContentPublishingOperationStatus.PublishTimeNeedsToBeInFuture => BadRequest(problemDetailsBuilder
                    .WithTitle("Publish time needs to be higher than the current time")
                    .WithDetail(
                        "Cannot handle a publish time that is not after the current server time.")
                    .Build()),
                ContentPublishingOperationStatus.UpublishTimeNeedsToBeInFuture => BadRequest(problemDetailsBuilder
                    .WithTitle("Unpublish time needs to be higher than the current time")
                    .WithDetail(
                        "Cannot handle an unpublish time that is not after the current server time.")
                    .Build()),
                ContentPublishingOperationStatus.CannotUnpublishWhenReferenced => BadRequest(problemDetailsBuilder
                    .WithTitle("Cannot unpublish document when it's referenced somewhere else.")
                    .WithDetail(
                        "Cannot unpublish a referenced document, while the setting ContentSettings.DisableUnpublishWhenReferenced is enabled.")
                    .Build()),
                ContentPublishingOperationStatus.FailedBranch => BadRequest(problemDetailsBuilder
                    .WithTitle("Failed branch operation")
                    .WithDetail("One or more items in the branch could not complete the operation.")
                    .WithExtension("failedBranchItems", failedBranchItems?.Select(item => new DocumentPublishBranchItemResult { Id = item.Key, OperationStatus = item.OperationStatus }) ?? [])
                    .Build()),
                ContentPublishingOperationStatus.Failed => BadRequest(
                    problemDetailsBuilder
                        .WithTitle("Publish or unpublish failed")
                        .WithDetail(
                            "An unspecified error occurred while (un)publishing. Please check the logs for additional information.")
                        .Build()),
                ContentPublishingOperationStatus.TaskResultNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The result of the submitted task could not be found")
                    .Build()),

                _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
            });

    protected IActionResult ContentEditingOperationStatusResult<TContentModelBase, TValueModel, TVariantModel>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<TValueModel, TVariantModel>
        where TValueModel : ValueModelBase
        where TVariantModel : VariantModelBase
    {
        if (status is not ContentEditingOperationStatus.PropertyValidationError)
        {
            return ContentEditingOperationStatusResult(status);
        }

        var errors = new SortedDictionary<string, string[]>();

        var validationErrorExpressionRoot = $"$.{nameof(ContentModelBase<TValueModel, TVariantModel>.Values).ToFirstLowerInvariant()}";
        foreach (PropertyValidationError validationError in validationResult.ValidationErrors)
        {
            TValueModel? requestValue = requestModel.Values.FirstOrDefault(value =>
                value.Alias == validationError.Alias
                && value.Culture == validationError.Culture
                && value.Segment == validationError.Segment);
            if (requestValue is null)
            {
                errors.Add(
                    $"{validationErrorExpressionRoot}[{JsonPathExpression.MissingPropertyValue(validationError.Alias, validationError.Culture, validationError.Segment)}].{nameof(ValueModelBase.Value)}",
                    validationError.ErrorMessages);
                continue;
            }

            var index = requestModel.Values.IndexOf(requestValue);
            errors.Add(
                $"$.{nameof(ContentModelBase<TValueModel, TVariantModel>.Values).ToFirstLowerInvariant()}[{index}].{nameof(ValueModelBase.Value).ToFirstLowerInvariant()}{validationError.JsonPath}",
                validationError.ErrorMessages);
        }

        return OperationStatusResult(status, problemDetailsBuilder
            => BadRequest(problemDetailsBuilder
                .WithTitle("Validation failed")
                .WithDetail("One or more properties did not pass validation")
                .WithRequestModelErrors(errors)
                .Build()));
    }
}
