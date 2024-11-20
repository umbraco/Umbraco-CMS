using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Umbraco.Cms.Api.Delivery.Controllers.Security;

[ApiVersion("1.0")]
[VersionedDeliveryApiRoute(Common.Security.Paths.MemberApi.EndpointTemplate)]
[ApiExplorerSettings(IgnoreApi = true)]
public class MemberController : DeliveryApiControllerBase
{
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly IMemberManager _memberManager;
    private readonly IMemberClientCredentialsManager _memberClientCredentialsManager;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly ILogger<MemberController> _logger;


    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
    public MemberController(
        IHttpContextAccessor httpContextAccessor,
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<MemberController> logger)
        : this(memberSignInManager, memberManager, StaticServiceProvider.Instance.GetRequiredService<IMemberClientCredentialsManager>(), deliveryApiSettings, logger)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
    public MemberController(
        IHttpContextAccessor httpContextAccessor,
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        IMemberClientCredentialsManager memberClientCredentialsManager,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<MemberController> logger)
        : this(memberSignInManager, memberManager, memberClientCredentialsManager, deliveryApiSettings, logger)
    {
    }

    [ActivatorUtilitiesConstructor]
    public MemberController(
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        IMemberClientCredentialsManager memberClientCredentialsManager,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<MemberController> logger)
    {
        _memberSignInManager = memberSignInManager;
        _memberManager = memberManager;
        _memberClientCredentialsManager = memberClientCredentialsManager;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    [HttpGet("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize()
    {
        // the Authorize endpoint is not allowed unless authorization code flow is enabled.
        if (_deliveryApiSettings.MemberAuthorization?.AuthorizationCodeFlow?.Enabled is not true)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "Not allowed", ErrorDescription = "Member authorization is not allowed."
            });
        }

        OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "No context found", ErrorDescription = "Unable to obtain context from the current request."
            });
        }

        // make sure this endpoint ONLY handles member authentication
        if (request.ClientId is not Constants.OAuthClientIds.Member)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "Invalid 'client ID'", ErrorDescription = "The specified 'client_id' is not valid."
            });
        }

        return request.IdentityProvider.IsNullOrWhiteSpace()
            ? await AuthorizeInternal(request)
            : await AuthorizeExternal(request);
    }

    [HttpPost("token")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Token()
    {
        OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "No context found", ErrorDescription = "Unable to obtain context from the current request."
            });
        }

        // authorization code flow or refresh token flow?
        if ((request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType()) && _deliveryApiSettings.MemberAuthorization?.AuthorizationCodeFlow?.Enabled is true)
        {
            // attempt to authorize against the supplied the authorization code
            AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return authenticateResult is { Succeeded: true, Principal: not null }
                ? new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, authenticateResult.Principal)
                : BadRequest(new OpenIddictResponse
                {
                    Error = "Authorization failed", ErrorDescription = "The supplied authorization could not be verified."
                });
        }

        // client credentials flow?
        if (request.IsClientCredentialsGrantType() && _deliveryApiSettings.MemberAuthorization?.ClientCredentialsFlow?.Enabled is true)
        {
            // if we get here, the client ID and secret are valid (verified by OpenIddict)

            MemberIdentityUser? member = await _memberClientCredentialsManager.FindMemberAsync(request.ClientId!);
            return member is not null
                ? await SignInMember(member, request)
                : BadRequest(new OpenIddictResponse
                {
                    Error = "Authorization failed", ErrorDescription = "Invalid 'client_id' or client configuration."
                });
        }

        throw new InvalidOperationException("The requested grant type is not supported.");
    }

    [HttpGet("signout")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Signout()
    {
        await _memberSignInManager.SignOutAsync();
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> AuthorizeInternal(OpenIddictRequest request)
    {
        // retrieve the user principal stored in the authentication cookie.
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        var userName = cookieAuthResult.Succeeded
            ? cookieAuthResult.Principal?.Identity?.Name
            : null;

        if (userName is null)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        MemberIdentityUser? member = await _memberManager.FindByNameAsync(userName);
        if (member is null)
        {
            _logger.LogError("The member with username {userName} was successfully authorized, but could not be retrieved by the member manager", userName);
            return BadRequest(new OpenIddictResponse
            {
                Error = "Authorization failed", ErrorDescription = "The member associated with the supplied 'client_id' could not be found."
            });
        }

        return await SignInMember(member, request);
    }

    private async Task<IActionResult> AuthorizeExternal(OpenIddictRequest request)
    {
        var provider = request.IdentityProvider ?? throw new ArgumentException("No identity provider found in request", nameof(request));
        ExternalLoginInfo? loginInfo = await _memberSignInManager.GetExternalLoginInfoAsync();

        if (loginInfo?.Principal is null)
        {
            AuthenticationProperties properties = _memberSignInManager.ConfigureExternalAuthenticationProperties(provider, null);
            return Challenge(properties, provider);
        }

        // NOTE: if we're going to support 2FA for members, we need to:
        //       - use SecuritySettings.MemberBypassTwoFactorForExternalLogins instead of the hardcoded value (true) for "bypassTwoFactor".
        //       - handle IdentitySignInResult.TwoFactorRequired
        IdentitySignInResult result = await _memberSignInManager.ExternalLoginSignInAsync(loginInfo, false, true);
        if (result == IdentitySignInResult.Success)
        {
            // get the member and perform sign-in
            MemberIdentityUser? member = await _memberManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            if (member is null)
            {
                _logger.LogError("A member was successfully authorized using external authentication, but could not be retrieved by the member manager");
                return BadRequest(new OpenIddictResponse
                {
                    Error = "Authorization failed", ErrorDescription = "The member associated with the supplied 'client_id' could not be found."
                });
            }

            // update member authentication tokens if succeeded
            await _memberSignInManager.UpdateExternalAuthenticationTokensAsync(loginInfo);
            return await SignInMember(member, request);
        }

        var errorProperties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccessDenied,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The member is not allowed to access this resource."
        });
        return Forbid(errorProperties, provider);
    }

    private async Task<IActionResult> SignInMember(MemberIdentityUser member, OpenIddictRequest request)
    {
        ClaimsPrincipal memberPrincipal = await _memberSignInManager.CreateUserPrincipalAsync(member);
        memberPrincipal.SetClaim(OpenIddictConstants.Claims.Subject, member.Key.ToString());

        IList<string> roles = await _memberManager.GetRolesAsync(member);
        memberPrincipal.SetClaim(Constants.OAuthClaims.MemberKey, member.Key.ToString());
        memberPrincipal.SetClaim(Constants.OAuthClaims.MemberRoles, string.Join(",", roles));

        Claim[] claims = memberPrincipal.Claims.ToArray();
        foreach (Claim claim in claims.Where(claim => claim.Type is not Constants.Security.SecurityStampClaimType))
        {
            claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
        }

        // "openid" and "offline_access" are the only scopes allowed for members; explicitly ensure we only add those
        // NOTE: the "offline_access" scope is required to use refresh tokens
        IEnumerable<string> allowedScopes = request
            .GetScopes()
            .Intersect(new[] { OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.OfflineAccess });
        memberPrincipal.SetScopes(allowedScopes);

        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, memberPrincipal);
    }
}
