﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class ChangePasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ChangePasswordUserController(
        IUserService userService,
        IUmbracoMapper mapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _mapper = mapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    [HttpPost("{id:guid}/change-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ChangePasswordUserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordUserRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.AdminUserEditsRequireAdmin);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var passwordModel = new ChangeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            UserKey = id,
        };

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(CurrentUserKey(_backOfficeSecurityAccessor), passwordModel);

        return response.Success
            ? Ok(_mapper.Map<ChangePasswordUserResponseModel>(response.Result))
            : UserOperationStatusResult(response.Status, response.Result);
    }
}
