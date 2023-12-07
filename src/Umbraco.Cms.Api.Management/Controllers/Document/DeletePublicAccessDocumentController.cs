﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class DeletePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessService _publicAccessService;

    public DeletePublicAccessDocumentController(IAuthorizationService authorizationService, IPublicAccessService publicAccessService)
    {
        _authorizationService = authorizationService;
        _publicAccessService = publicAccessService;
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var resource = new ContentPermissionResource(id, ActionProtect.ActionLetter);
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(User, resource,AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        await _publicAccessService.DeleteAsync(id);

        return Ok();
    }
}
