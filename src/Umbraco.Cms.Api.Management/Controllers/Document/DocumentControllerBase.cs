using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Document)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentControllerBase : ContentControllerBase
{
    protected IActionResult DocumentNotFound()
        => OperationStatusResult(ContentEditingOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
                .Build()));

    protected IActionResult DocumentEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<DocumentValueModel, DocumentVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, DocumentValueModel, DocumentVariantRequestModel>(status, requestModel, validationResult);

    protected IActionResult DocumentPublishingOperationStatusResult(
        ContentPublishingOperationStatus status,
        IEnumerable<string>? invalidPropertyAliases = null,
        IEnumerable<ContentPublishingBranchItemResult>? failedBranchItems = null)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
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
                .WithExtension("failedBranchItems", failedBranchItems?.Select(item => new DocumentPublishBranchItemResult
                    {
                        Id = item.Key,
                        OperationStatus = item.OperationStatus
                    }) ?? Enumerable.Empty<DocumentPublishBranchItemResult>())
                .Build()),
            ContentPublishingOperationStatus.Failed => BadRequest(problemDetailsBuilder
                .WithTitle("Publish or unpublish failed")
                .WithDetail(
                    "An unspecified error occurred while (un)publishing. Please check the logs for additional information.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown content operation status."),
        });

    protected IActionResult PublicAccessOperationStatusResult(PublicAccessOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            PublicAccessOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
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
            PublicAccessOperationStatus.EntryNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Entry not found")
                .WithDetail("The specified entry was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });

    protected IActionResult ContentQueryOperationStatusResult(ContentQueryOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentQueryOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content query status.")
                .Build()),
        });
}
