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

[ApiVersion("1.0")]
public class PublishDocumentWithDescendantsResultController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;

    public PublishDocumentWithDescendantsResultController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
    }

    [HttpGet("{id:guid}/publish-with-descendants/result/{taskId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishWithDescendantsResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
            return Ok(new PublishWithDescendantsResultModel
            {
                TaskId = taskId,
                IsComplete = false
            });
        };

        // If completed, get the result and return the status.
        Attempt<ContentPublishingBranchResult, ContentPublishingOperationStatus> attempt = await _contentPublishingService.GetPublishBranchResultAsync(taskId);
        return attempt.Success
            ? Ok(new PublishWithDescendantsResultModel
            {
                TaskId = taskId,
                IsComplete = true
            })
            : DocumentPublishingOperationStatusResult(attempt.Status, failedBranchItems: attempt.Result.FailedItems);
    }
}
