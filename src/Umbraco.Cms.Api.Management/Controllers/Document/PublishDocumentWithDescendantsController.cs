using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishWithDescendants(Guid id, PublishDocumentWithDescendantsRequestModel requestModel)
    {
        var resource = new ContentPermissionResource(id, ActionPublish.ActionLetter);
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, resource,
            $"New{AuthorizationPolicies.ContentPermissionByResource}");

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IDictionary<Guid, ContentPublishingOperationStatus>> attempt = await _contentPublishingService.PublishBranchAsync(
            id,
            requestModel.Cultures,
            requestModel.IncludeUnpublishedDescendants,
            CurrentUserKey(_backOfficeSecurityAccessor));

        // FIXME: when we get to implement proper validation handling, this should return a collection of status codes by key (based on attempt.Result)
        return attempt.Success
            ? Ok()
            : ContentPublishingOperationStatusResult(
                attempt.Result?.Values.FirstOrDefault(r => r is not ContentPublishingOperationStatus.Success)
                ?? throw new NotSupportedException("The attempt was not successful - at least one result value should be unsuccessful too"));
    }
}
