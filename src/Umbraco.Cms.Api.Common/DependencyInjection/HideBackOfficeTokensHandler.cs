using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     Handles secure storage of back-office authentication tokens in HTTP-only cookies.
/// </summary>
/// <remarks>
///     This handler intercepts OpenIddict token responses for the back-office client and stores
///     access tokens, refresh tokens, and PKCE codes in encrypted HTTP-only cookies. The tokens
///     are redacted from the response to prevent client-side JavaScript access.
/// </remarks>
internal sealed class HideBackOfficeTokensHandler
    : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyTokenResponseContext>,
        IOpenIddictServerHandler<OpenIddictServerEvents.ApplyAuthorizationResponseContext>,
        IOpenIddictServerHandler<OpenIddictServerEvents.ExtractTokenRequestContext>,
        IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessAuthenticationContext>,
        INotificationHandler<UserLogoutSuccessNotification>
{
    private const string RedactedTokenValue = "[redacted]";

    // The __Host- prefix enforces secure cookies at browser level (requires Secure, Path=/, no Domain).
    // For local development over HTTP, we use a simpler prefix to avoid browser rejection.
    private const string SecureCookiePrefix = "__Host-";
    private const string AccessTokenCookieName = "umbAccessToken";
    private const string RefreshTokenCookieName = "umbRefreshToken";
    private const string PkceCodeCookieName = "umbPkceCode";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly BackOfficeTokenCookieSettings _backOfficeTokenCookieSettings;
    private readonly GlobalSettings _globalSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HideBackOfficeTokensHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="dataProtectionProvider">The data protection provider for encrypting cookie values.</param>
    /// <param name="backOfficeTokenCookieSettings">The back-office token cookie settings.</param>
    /// <param name="globalSettings">The global settings.</param>
    public HideBackOfficeTokensHandler(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<BackOfficeTokenCookieSettings> backOfficeTokenCookieSettings,
        IOptions<GlobalSettings> globalSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
        _backOfficeTokenCookieSettings = backOfficeTokenCookieSettings.Value;
        _globalSettings = globalSettings.Value;
    }

    /// <summary>
    /// This is invoked when tokens (access and refresh tokens) are issued to a client. For the back-office client,
    /// we will intercept the response, write the tokens from the response into HTTP-only cookies, and redact the
    /// tokens from the response, so they are not exposed to the client.
    /// </summary>
    public ValueTask HandleAsync(OpenIddictServerEvents.ApplyTokenResponseContext context)
    {
        if (context.Request?.ClientId is not Constants.OAuthClientIds.BackOffice)
        {
            // Only ever handle the back-office client.
            return ValueTask.CompletedTask;
        }

        HttpContext httpContext = GetHttpContext();

        if (context.Response.AccessToken is not null)
        {
            SetCookie(httpContext, AccessTokenCookieName, context.Response.AccessToken);
            context.Response.AccessToken = RedactedTokenValue;
        }

        if (context.Response.RefreshToken is not null)
        {
            SetCookie(httpContext, RefreshTokenCookieName, context.Response.RefreshToken);
            context.Response.RefreshToken = RedactedTokenValue;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// This is invoked when a PKCE code is issued to the client. For the back-office client, we will intercept the
    /// response, write the PKCE code from the response into a HTTP-only cookie, and redact the code from the response,
    /// so it's not exposed to the client.
    /// </summary>
    public ValueTask HandleAsync(OpenIddictServerEvents.ApplyAuthorizationResponseContext context)
    {
        if (context.Request?.ClientId is not Constants.OAuthClientIds.BackOffice)
        {
            // Only ever handle the back-office client.
            return ValueTask.CompletedTask;
        }

        if (context.Response.Code is not null)
        {
            SetCookie(GetHttpContext(), PkceCodeCookieName, context.Response.Code);
            context.Response.Code = RedactedTokenValue;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// This is invoked when requesting new tokens.
    /// </summary>
    public ValueTask HandleAsync(OpenIddictServerEvents.ExtractTokenRequestContext context)
    {
        if (context.Request?.ClientId != Constants.OAuthClientIds.BackOffice)
        {
            // Only ever handle the back-office client.
            return ValueTask.CompletedTask;
        }

        HttpContext httpContext = GetHttpContext();

        // Handle when the PKCE code is being exchanged for an access token.
        if (context.Request.Code == RedactedTokenValue
            && TryGetCookie(httpContext, PkceCodeCookieName, out var code))
        {
            context.Request.Code = code;

            // We won't need the PKCE cookie after this, let's remove it.
            RemoveCookie(httpContext, PkceCodeCookieName);
        }
        else
        {
            // PCKE codes should always be redacted. If we got here, someone might be trying to pass another PKCE
            // code. For security reasons, explicitly discard the code (if any) to be on the safe side.
            context.Request.Code = null;
        }

        // Handle when a refresh token is being exchanged for a new access token.
        if (context.Request.RefreshToken == RedactedTokenValue
            && TryGetCookie(httpContext, RefreshTokenCookieName, out var refreshToken))
        {
            context.Request.RefreshToken = refreshToken;
        }
        else
        {
            // If we got here, either the refresh token was not redacted, or nothing was found in the refresh token cookie.
            // If OpenIddict found a refresh token, it could be an old token that is potentially still valid. For security
            // reasons, we cannot accept that; at this point, we expect the refresh tokens to be explicitly redacted.
            context.Request.RefreshToken = null;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// This is invoked when extracting the auth context for a client request.
    /// </summary>
    public ValueTask HandleAsync(OpenIddictValidationEvents.ProcessAuthenticationContext context)
    {
        // For the back-office client, this only happens when an access token is sent to the API.
        if (context.AccessToken != RedactedTokenValue)
        {
            return ValueTask.CompletedTask;
        }

        if (TryGetCookie(GetHttpContext(), AccessTokenCookieName, out var accessToken))
        {
            context.AccessToken = accessToken;
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Handle(UserLogoutSuccessNotification notification)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            // For some reason there is no ambient HTTP context, so we can't clean up the cookies.
            // This is OK, because the tokens in the cookies have already been revoked at user sign-out,
            // so the cookie clean-up is mostly cosmetic.
            return;
        }

        RemoveCookie(httpContext, AccessTokenCookieName);
        RemoveCookie(httpContext, RefreshTokenCookieName);
    }

    private HttpContext GetHttpContext()
        => _httpContextAccessor.GetRequiredHttpContext();

    private string GetCookieKey(HttpContext httpContext, string cookieName)
        => _globalSettings.UseHttps || httpContext.Request.IsHttps
            ? $"{SecureCookiePrefix}{cookieName}"
            : cookieName;

    private void SetCookie(HttpContext httpContext, string cookieName, string value)
    {
        var key = GetCookieKey(httpContext, cookieName);
        var cookieValue = EncryptionHelper.Encrypt(value, _dataProtectionProvider);

        RemoveCookie(httpContext, cookieName);
        httpContext.Response.Cookies.Append(key, cookieValue, GetCookieOptions(httpContext));
    }

    private void RemoveCookie(HttpContext httpContext, string cookieName)
    {
        var key = GetCookieKey(httpContext, cookieName);
        httpContext.Response.Cookies.Delete(key, GetCookieOptions(httpContext));
    }

    private CookieOptions GetCookieOptions(HttpContext httpContext) =>
        new()
        {
            // Prevent the client-side scripts from accessing the cookie.
            HttpOnly = true,

            // Mark the cookie as essential to the application, to enforce it despite any
            // data collection consent options. This aligns with how ASP.NET Core Identity
            // does when writing cookies for cookie authentication.
            IsEssential = true,

            // Cookie path must be root for optimal security.
            Path = "/",

            // For optimal security, the cooke must be secure. However, Umbraco allows for running development
            // environments over HTTP, so we need to take that into account here.
            // Thus, we will make the cookie secure if:
            // - HTTPS is explicitly enabled by config (default for production environments), or
            // - The current request is over HTTPS (meaning the environment supports it regardless of config).
            Secure = _globalSettings.UseHttps || httpContext.Request.IsHttps,

            // SameSite is configurable (see BackOfficeTokenCookieSettings for defaults):
            SameSite = ParseSameSiteMode(_backOfficeTokenCookieSettings.SameSite),
        };

    private bool TryGetCookie(HttpContext httpContext, string cookieName, [NotNullWhen(true)] out string? value)
    {
        var key = GetCookieKey(httpContext, cookieName);
        if (httpContext.Request.Cookies.TryGetValue(key, out var cookieValue))
        {
            value = EncryptionHelper.Decrypt(cookieValue, _dataProtectionProvider);
            return true;
        }

        value = null;
        return false;
    }

    private static SameSiteMode ParseSameSiteMode(string sameSiteMode) =>
        Enum.TryParse(sameSiteMode, ignoreCase: true, out SameSiteMode result)
            ? result
            : throw new ArgumentException($"The provided {nameof(sameSiteMode)} value could not be parsed into as SameSiteMode value.", nameof(sameSiteMode));
}
