using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// API controller that lists the available external login providers for the currently authenticated user.
/// </summary>
[ApiVersion("1.0")]
public class ListExternalLoginProvidersCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IBackOfficeExternalLoginService _backOfficeExternalLoginService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListExternalLoginProvidersCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    /// <param name="backOfficeExternalLoginService">The service used to manage external login providers for the back office.</param>
    /// <param name="mapper">The Umbraco object mapper instance.</param>
    public ListExternalLoginProvidersCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeExternalLoginService backOfficeExternalLoginService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _backOfficeExternalLoginService = backOfficeExternalLoginService;
        _mapper = mapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("login-providers")]
    [ProducesResponseType(typeof(IEnumerable<UserExternalLoginProviderModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Lists external login providers.")]
    [EndpointDescription("Gets a list of configured external login providers for authentication.")]
    public async Task<IActionResult> ListTwoFactorProvidersForCurrentUser(CancellationToken cancellationToken)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus> result =
            await _backOfficeExternalLoginService.ExternalLoginStatusForUserAsync(userKey);

        return result.Success
            ? Ok(_mapper.MapEnumerable<UserExternalLoginProviderModel, UserExternalLoginProviderResponseModel>(result.Result))
            : ExternalLoginOperationStatusResult(result.Status);
    }
}
