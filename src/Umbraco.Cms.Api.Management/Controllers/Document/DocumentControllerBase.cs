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
        => ContentPublishingOperationStatusResult(status, invalidPropertyAliases, failedBranchItems);

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
