﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class MoveDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(Guid id, MoveDocumentRequestModel moveDocumentRequestModel)
    {
        AuthorizationResult authorizationResult;

        if (moveDocumentRequestModel.TargetId.HasValue is false)
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { ActionMove.ActionLetter },
                $"New{AuthorizationPolicies.ContentPermissionAtRoot}");
        }
        else
        {
            var resource = new ContentPermissionResource(moveDocumentRequestModel.TargetId.Value, ActionMove.ActionLetter);
            authorizationResult = await _authorizationService.AuthorizeAsync(User, resource,
                $"New{AuthorizationPolicies.ContentPermissionByResource}");
        }

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.MoveAsync(
            id,
            moveDocumentRequestModel.TargetId,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
