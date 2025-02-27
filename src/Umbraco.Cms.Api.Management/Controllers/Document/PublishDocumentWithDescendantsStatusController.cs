using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class PublishDocumentWithDescendantsStatusController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;

    public PublishDocumentWithDescendantsStatusController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
    }

    [HttpGet("{id:guid}/publish-with-descendants/status/{taskId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishWithDescendantsStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishWithDescendantsStatus(CancellationToken cancellationToken, Guid id, Guid taskId)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.Branch(ActionPublish.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var isPublishing = await _contentPublishingService.IsPublishingBranchAsync(taskId);
        return Ok(new PublishWithDescendantsStatusModel
        {
            TaskId = taskId,
            IsPublishing = isPublishing
        });
    }
}
