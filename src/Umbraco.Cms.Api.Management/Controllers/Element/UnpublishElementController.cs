using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class UnpublishElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementPublishingService _elementPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UnpublishElementController(
        IAuthorizationService authorizationService,
        IElementPublishingService elementPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _elementPublishingService = elementPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/unpublish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unpublish(CancellationToken cancellationToken, Guid id, UnpublishElementRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(
                ActionElementUnpublish.ActionLetter,
                id,
                requestModel.Cultures ?? Enumerable.Empty<string>()),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentPublishingOperationStatus> attempt = await _elementPublishingService.UnpublishAsync(
            id,
            requestModel.Cultures,
            CurrentUserKey(_backOfficeSecurityAccessor));
        return attempt.Success
            ? Ok()
            : ElementPublishingOperationStatusResult(attempt.Result);
    }
}
