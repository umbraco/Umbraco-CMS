using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

    /// <summary>
    /// API controller responsible for managing calculated start nodes for users in Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class CalculatedStartNodesUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculatedStartNodesUserController"/> class,
    /// which manages user-related operations involving calculated start nodes.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user access to controller actions.</param>
    /// <param name="userService">Service for managing user data and operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    public CalculatedStartNodesUserController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpGet("{id:guid}/calculate-start-nodes")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(CalculatedUserStartNodesResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Calculates start nodes for users.")]
    [EndpointDescription("Calculates the start nodes for the users identified by the provided Ids based on their permissions.")]
    public async Task<IActionResult> CalculatedStartNodes(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IUser? user = await _userService.GetAsync(id);

        if (user is null)
        {
            return UserOperationStatusResult(UserOperationStatus.UserNotFound);
        }

        CalculatedUserStartNodesResponseModel responseModel = await _userPresentationFactory.CreateCalculatedUserStartNodesResponseModelAsync(user);
        return Ok(responseModel);
    }
}
