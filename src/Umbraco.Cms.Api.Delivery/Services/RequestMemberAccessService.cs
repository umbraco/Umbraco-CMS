using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestMemberAccessService : IRequestMemberAccessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublicAccessChecker _publicAccessChecker;
    private readonly DeliveryApiSettings _deliveryApiSettings;

    public RequestMemberAccessService(
        IHttpContextAccessor httpContextAccessor,
        IPublicAccessService publicAccessService,
        IPublicAccessChecker publicAccessChecker,
        IOptions<DeliveryApiSettings> deliveryApiSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _publicAccessService = publicAccessService;
        _publicAccessChecker = publicAccessChecker;

        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    public async Task<PublicAccessStatus> MemberHasAccessToAsync(IPublishedContent content)
    {
        PublicAccessEntry? publicAccessEntry = _publicAccessService.GetEntryForContent(content.Path);
        if (publicAccessEntry is null)
        {
            return PublicAccessStatus.AccessAccepted;
        }

        ClaimsPrincipal? requestPrincipal = await GetRequestPrincipal();
        if (requestPrincipal is null)
        {
            return PublicAccessStatus.NotLoggedIn;
        }

        return await _publicAccessChecker.HasMemberAccessToContentAsync(content.Id, requestPrincipal);
    }

    public async Task<ProtectedAccess> MemberAccessAsync()
    {
        ClaimsPrincipal? requestPrincipal = await GetRequestPrincipal();
        return new ProtectedAccess(MemberKey(requestPrincipal), MemberRoles(requestPrincipal));
    }

    private async Task<ClaimsPrincipal?> GetRequestPrincipal()
    {
        // exit fast if no member authorization is enabled whatsoever
        if (_deliveryApiSettings.MemberAuthorizationIsEnabled() is false)
        {
            return null;
        }

        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();
        AuthenticateResult result = await httpContext.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        return result.Succeeded
            ? result.Principal
            : null;
    }

    private static Guid? MemberKey(ClaimsPrincipal? claimsPrincipal)
        => claimsPrincipal is not null && Guid.TryParse(claimsPrincipal.GetClaim(Constants.OAuthClaims.MemberKey), out Guid memberKey)
            ? memberKey
            : null;

    private static string[]? MemberRoles(ClaimsPrincipal? claimsPrincipal)
        => claimsPrincipal?.GetClaim(Constants.OAuthClaims.MemberRoles)?.Split(Constants.CharArrays.Comma);
}
