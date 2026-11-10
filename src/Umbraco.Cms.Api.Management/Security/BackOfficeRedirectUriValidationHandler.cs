using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Restricts the redirect URIs accepted for the back-office OpenIddict clients to the host serving the request,
///     or to a host that has been explicitly configured.
/// </summary>
/// <remarks>
///     The redirect URIs registered for these clients are derived from the hosts the back-office has been requested on,
///     so the registration alone cannot be trusted to identify a legitimate redirect target. A browser always sends the
///     host it is actually talking to, so binding the redirect URI to that host (or to configuration) ensures that
///     authorization codes and post-logout redirects can only be delivered to the site itself.
/// </remarks>
internal sealed class BackOfficeRedirectUriValidationHandler
    : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>,
        IOpenIddictServerHandler<OpenIddictServerEvents.ValidateEndSessionRequestContext>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<SecuritySettings> _securitySettings;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeRedirectUriValidationHandler"/> class.
    /// </summary>
    public BackOfficeRedirectUriValidationHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<SecuritySettings> securitySettings,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _securitySettings = securitySettings;
        _webRoutingSettings = webRoutingSettings;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateAuthorizationRequestContext context)
    {
        var clientId = context.ClientId ?? context.Request.ClientId;
        if (clientId is not (Constants.OAuthClientIds.BackOffice or Constants.OAuthClientIds.OpenApiUi))
        {
            return ValueTask.CompletedTask;
        }

        if (clientId is Constants.OAuthClientIds.BackOffice
            && context.Request.ResponseMode.IsNullOrWhiteSpace() is false
            && context.Request.ResponseMode is not OpenIddictConstants.ResponseModes.Query)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidRequest,
                description: "The specified 'response_mode' is not supported.");
            return ValueTask.CompletedTask;
        }

        if (IsAllowedRedirectUri(context.Request.RedirectUri) is false)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidRequest,
                description: "The specified 'redirect_uri' is not valid for this host.");
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateEndSessionRequestContext context)
    {
        if (context.Request.PostLogoutRedirectUri.IsNullOrWhiteSpace()
            || IsBackOfficeEndSessionRequest() is false)
        {
            return ValueTask.CompletedTask;
        }

        if (IsAllowedRedirectUri(context.Request.PostLogoutRedirectUri) is false)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidRequest,
                description: "The specified 'post_logout_redirect_uri' is not valid for this host.");
        }

        return ValueTask.CompletedTask;
    }

    private bool IsBackOfficeEndSessionRequest()
        => _httpContextAccessor.HttpContext?.Request.Path.Equals(Paths.BackOfficeApi.LogoutEndpoint, StringComparison.OrdinalIgnoreCase) is true;

    private bool IsAllowedRedirectUri(string? redirectUri)
    {
        // A missing or malformed redirect URI is rejected by OpenIddict itself.
        if (TryGetAuthority(redirectUri, out var redirectAuthority) is false)
        {
            return true;
        }

        return GetAllowedAuthorities().Contains(redirectAuthority, StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<string> GetAllowedAuthorities()
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request is not null && TryGetAuthority($"{request.Scheme}://{request.Host}", out var requestAuthority))
        {
            yield return requestAuthority;
        }

        if (TryGetAuthority(_securitySettings.CurrentValue.BackOfficeHost?.ToString(), out var backOfficeHostAuthority))
        {
            yield return backOfficeHostAuthority;
        }

        if (TryGetAuthority(_webRoutingSettings.CurrentValue.UmbracoApplicationUrl, out var applicationUrlAuthority))
        {
            yield return applicationUrlAuthority;
        }
    }

    private static bool TryGetAuthority(string? uri, out string authority)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed) && parsed.IsAbsoluteUri)
        {
            authority = parsed.GetLeftPart(UriPartial.Authority);
            return true;
        }

        authority = string.Empty;
        return false;
    }
}
