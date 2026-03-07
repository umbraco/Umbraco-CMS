using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

    /// <summary>
    /// Controller responsible for publishing a document and all of its descendant documents in the content tree.
    /// </summary>
[ApiVersion("1.0")]
public class PublishDocumentWithDescendantsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishDocumentWithDescendantsController"/> class, which handles publishing a document and its descendants in the Umbraco CMS.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize publishing actions.</param>
    /// <param name="contentPublishingService">Service responsible for executing content publishing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and user information.</param>
    public PublishDocumentWithDescendantsController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Publishes the specified document and all of its descendants.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the root document to publish along with its descendants.</param>
    /// <param name="requestModel">The request model specifying cultures to publish and additional publishing options.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="PublishWithDescendantsResultModel"/> if the operation is accepted, or a <see cref="ProblemDetails"/> if the request is invalid or the document is not found.
    /// </returns>
    [HttpPut("{id:guid}/publish-with-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishWithDescendantsResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Publishes a document with its descendants.")]
    [EndpointDescription("Publishes a document and its descendants identified by the provided Id.")]
    public async Task<IActionResult> PublishWithDescendants(CancellationToken cancellationToken, Guid id, PublishDocumentWithDescendantsRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.Branch(ActionPublish.ActionLetter, id, requestModel.Cultures),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus> attempt = await _contentPublishingService.PublishBranchAsync(
            id,
            requestModel.Cultures,
            BuildPublishBranchFilter(requestModel),
            CurrentUserKey(_backOfficeSecurityAccessor),
            true);

        return attempt.Success && attempt.Result.AcceptedTaskId.HasValue
            ? Ok(
                new PublishWithDescendantsResultModel
                {
                    TaskId = attempt.Result.AcceptedTaskId.Value,
                    IsComplete = false,
                })
            : DocumentPublishingOperationStatusResult(attempt.Status, failedBranchItems: attempt.Result.FailedItems);
    }

    private static PublishBranchFilter BuildPublishBranchFilter(PublishDocumentWithDescendantsRequestModel requestModel)
    {
        PublishBranchFilter publishBranchFilter = PublishBranchFilter.Default;
        if (requestModel.IncludeUnpublishedDescendants)
        {
            publishBranchFilter |= PublishBranchFilter.IncludeUnpublished;
        }

        return publishBranchFilter;
    }
}
