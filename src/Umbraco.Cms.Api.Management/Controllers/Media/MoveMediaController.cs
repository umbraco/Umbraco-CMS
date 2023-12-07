using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class MoveMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(Guid id, MoveMediaRequestModel moveDocumentRequestModel)
    {
        AuthorizationResult authorizationResult;

        if (moveDocumentRequestModel.TargetId.HasValue is false)
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, $"New{AuthorizationPolicies.MediaPermissionAtRoot}");
        }
        else
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { moveDocumentRequestModel.TargetId.Value },
                $"New{AuthorizationPolicies.MediaPermissionByResource}");
        }

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IMedia?, ContentEditingOperationStatus> result = await _mediaEditingService.MoveAsync(
            id,
            moveDocumentRequestModel.TargetId,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
