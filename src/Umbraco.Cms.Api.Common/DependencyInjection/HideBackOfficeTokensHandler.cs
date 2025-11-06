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

internal sealed class HideBackOfficeTokensHandler
    : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyTokenResponseContext>,
        IOpenIddictServerHandler<OpenIddictServerEvents.ExtractTokenRequestContext>,
        IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessAuthenticationContext>,
        INotificationHandler<UserLogoutSuccessNotification>
{
    private const string RedactedTokenValue = "[redacted]";
    private const string AccessTokenCookieKey = "umbAccessToken";
    private const string RefreshTokenCookieKey = "umbRefreshToken";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly GlobalSettings _globalSettings;

    public HideBackOfficeTokensHandler(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider, IOptions<GlobalSettings> globalSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
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
            SetCookie(httpContext, AccessTokenCookieKey, context.Response.AccessToken);
            context.Response.AccessToken = RedactedTokenValue;
        }

        if (context.Response.RefreshToken is not null)
        {
            SetCookie(httpContext, RefreshTokenCookieKey, context.Response.RefreshToken);
            context.Response.RefreshToken = RedactedTokenValue;
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

        // For the back-office client, this only happens when a refresh token is being exchanged for a new access token.
        if (context.Request.RefreshToken == RedactedTokenValue
            && TryGetCookie(RefreshTokenCookieKey, out var refreshToken))
        {
            context.Request.RefreshToken = refreshToken;
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

        if (TryGetCookie(AccessTokenCookieKey, out var accessToken))
        {
            context.AccessToken = accessToken;
        }

        return ValueTask.CompletedTask;
    }

    public void Handle(UserLogoutSuccessNotification notification)
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            // For some reason there is no ambient HTTP context, so we can't clean up the cookies.
            // This is OK, because the tokens in the cookies have already been revoked at user sign-out,
            // so the cookie clean-up is mostly cosmetic.
            return;
        }

        context.Response.Cookies.Delete(AccessTokenCookieKey);
        context.Response.Cookies.Delete(RefreshTokenCookieKey);
    }

    private HttpContext GetHttpContext()
        => _httpContextAccessor.GetRequiredHttpContext();

    private void SetCookie(HttpContext httpContext, string key, string value)
    {
        var cookieValue = EncryptionHelper.Encrypt(value, _dataProtectionProvider);

        httpContext.Response.Cookies.Delete(key);
        httpContext.Response.Cookies.Append(
            key,
            cookieValue,
            new CookieOptions
            {
                // Prevent the client-side scripts from accessing the cookie.
                HttpOnly = true,

                // Strictly only ever pass the cookie to the issuing (back-office) host.
                SameSite = SameSiteMode.Strict,

                // Use a secure cookie if HTTPS is enforced by the global settings, which
                // should always be the case for production environments.
                Secure = _globalSettings.UseHttps,

                // The cookie cannot be scoped to a path, because stand-alone clients may use a
                // different path to the APIs (e.g. when hosting the back-office on a different host).
                Path = "/",

                // Mark the cookie as essential to the application, to enforce it despite any
                // data collection consent options. This aligns with how ASP.NET Core Identity
                // does when writing cookies for cookie authentication.
                IsEssential = true,
            });
    }

    private bool TryGetCookie(string key, [NotNullWhen(true)] out string? value)
    {
        if (GetHttpContext().Request.Cookies.TryGetValue(key, out var cookieValue))
        {
            value = EncryptionHelper.Decrypt(cookieValue, _dataProtectionProvider);
            return true;
        }

        value = null;
        return false;
    }
}
