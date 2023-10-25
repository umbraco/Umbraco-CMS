using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Api.Delivery.Controllers.Security;

[ApiVersion("1.0")]
[ApiController]
[VersionedDeliveryApiRoute(Common.Security.Paths.MemberApi.EndpointTemplate)]
[ApiExplorerSettings(IgnoreApi = true)]
public class MemberController : DeliveryApiControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly IMemberManager _memberManager;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly ILogger<MemberController> _logger;

    public MemberController(
        IHttpContextAccessor httpContextAccessor,
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<MemberController> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _memberSignInManager = memberSignInManager;
        _memberManager = memberManager;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    [HttpGet("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize()
    {
        // in principle this is not necessary for now, since the member application has been removed, thus making
        // the member client ID invalid for the authentication code flow. However, if we ever add additional flows
        // to the API, we should perform this check, so we might as well include it upfront.
        if (_deliveryApiSettings.MemberAuthorizationIsEnabled() is false)
        {
            return BadRequest("Member authorization is not allowed.");
        }

        HttpContext context = _httpContextAccessor.GetRequiredHttpContext();
        OpenIddictRequest? request = context.GetOpenIddictServerRequest();
        if (request is null)
        {
            return BadRequest("Unable to obtain OpenID data from the current request.");
        }

        // make sure this endpoint ONLY handles member authentication
        if (request.ClientId is not Constants.OAuthClientIds.Member)
        {
            return BadRequest("The specified client ID cannot be used here.");
        }

        return request.IdentityProvider.IsNullOrWhiteSpace()
            ? await AuthorizeInternal(request)
            : await AuthorizeExternal(request);
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
            return BadRequest("The member could not be found.");
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
                return BadRequest("The member could not be found.");
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

        if (request.GetScopes().Contains(OpenIddictConstants.Scopes.OfflineAccess))
        {
            // "offline_access" scope is required to use refresh tokens
            memberPrincipal.SetScopes(OpenIddictConstants.Scopes.OfflineAccess);
        }

        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, memberPrincipal);
    }
}
