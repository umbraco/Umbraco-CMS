using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller that handles results of publishing a document and its descendants.
/// </summary>
[ApiVersion("1.0")]
public class PublishDocumentWithDescendantsResultController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishDocumentWithDescendantsResultController"/> class.
    /// </summary>
    /// <param name="authorizationService">An instance of <see cref="IAuthorizationService"/> used to check user permissions.</param>
    /// <param name="contentPublishingService">An instance of <see cref="IContentPublishingService"/> used to publish content and its descendants.</param>
    public PublishDocumentWithDescendantsResultController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
    }

    /// <summary>
    /// Retrieves the status and result of a publish operation for a document and its descendants.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document being published.</param>
    /// <param name="taskId">The unique identifier of the publish operation task.</param>
    /// <returns>An <see cref="IActionResult"/> containing the status and details of the publish operation, including whether it is complete and any relevant results.</returns>
    [HttpGet("{id:guid}/publish-with-descendants/result/{taskId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishWithDescendantsResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets the result of publishing a document with descendants.")]
    [EndpointDescription("Gets the status and result of a publish with descendants operation.")]
    public async Task<IActionResult> PublishWithDescendantsResult(CancellationToken cancellationToken, Guid id, Guid taskId)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.Branch(ActionPublish.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        // Check if the publishing task has completed, if not, return the status.
        var isPublishing = await _contentPublishingService.IsPublishingBranchAsync(taskId);
        if (isPublishing)
        {
            return Ok(
                new PublishWithDescendantsResultModel
                {
                    TaskId = taskId,
                    IsComplete = false,
                });
        }

        // If completed, get the result and return the status.
        Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus> attempt = await _contentPublishingService.GetPublishBranchResultAsync(taskId);
        return attempt.Success
            ? Ok(
                new PublishWithDescendantsResultModel
                {
                    TaskId = taskId,
                    IsComplete = true,
                })
            : DocumentPublishingOperationStatusResult(attempt.Status, failedBranchItems: attempt.Result.FailedItems);
    }
}
