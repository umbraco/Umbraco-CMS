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

[ApiVersion("1.0")]
public class PublishDocumentWithDescendantsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public PublishDocumentWithDescendantsController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/publish-with-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishWithDescendantsResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
