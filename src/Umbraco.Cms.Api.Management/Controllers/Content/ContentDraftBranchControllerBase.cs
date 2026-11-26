using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Content;

/// <summary>
/// Serves as the shared base controller for named draft branch operations across both Document and Element
/// draft branches in the Umbraco CMS Management API.
/// </summary>
public abstract class ContentDraftBranchControllerBase : ManagementApiControllerBase
{
    /// <summary>
    /// The lower-case entity name ("document" or "element") used to phrase operation-status error messages.
    /// </summary>
    protected abstract string EntityName { get; }

    /// <summary>
    /// Maps the failure statuses a named draft branch operation (create, update, promote, schedule, abandon)
    /// can produce.
    /// </summary>
    protected IActionResult MapFailure(ContentDraftBranchOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentDraftBranchOperationStatus.ContentNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle($"The {EntityName} could not be found")
                .Build()),
            ContentDraftBranchOperationStatus.BranchNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("The draft branch could not be found")
                .Build()),
            ContentDraftBranchOperationStatus.BranchVersionNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("The draft branch's own version could not be found")
                .Build()),
            ContentDraftBranchOperationStatus.NoDefaultDraft => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle($"The {EntityName} has no default draft to fork from")
                .Build()),
            ContentDraftBranchOperationStatus.SaveFailed => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Saving the draft branch failed")
                .Build()),
            ContentDraftBranchOperationStatus.CancelledByNotification => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Request cancelled by notification")
                .WithDetail("The request was cancelled by a notification handler.")
                .Build()),
            ContentDraftBranchOperationStatus.CultureMissing => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Culture missing")
                .WithDetail("A culture needs to be specified to execute the operation.")
                .Build()),
            ContentDraftBranchOperationStatus.InvalidCulture => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Invalid cultures specified")
                .WithDetail("A specified culture is not valid for the operation.")
                .Build()),
            _ => new ObjectResult(problemDetailsBuilder
                .WithTitle("Unknown draft branch operation status.")
                .Build()) { StatusCode = StatusCodes.Status500InternalServerError },
        });

    /// <summary>
    /// Maps the (narrow) set of failures that mapping an update model onto a draft branch's content can
    /// produce - see <see cref="Umbraco.Cms.Core.Services.IContentEditingService.MapUpdateAsync"/> and its
    /// Element equivalent. Unlike an ordinary content update, the content type itself is never user-supplied
    /// here (it's already resolved from the branch's own loaded content), so only content-type-shape
    /// mismatches are realistically possible.
    /// </summary>
    protected IActionResult MapContentEditingFailure(ContentEditingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentEditingOperationStatus.ContentTypeNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("The requested content type could not be found")
                .Build()),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            _ => new ObjectResult(problemDetailsBuilder
                .WithTitle("Unknown content editing operation status.")
                .Build()) { StatusCode = StatusCodes.Status500InternalServerError },
        });
}
