using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
///     Provides basic authentication via back-office credentials for public website access if configured for use and the
///     client IP is not allow listed.
/// </summary>
public class BasicAuthenticationMiddleware : IMiddleware
{
    private readonly IBasicAuthService _basicAuthService;
    private readonly IRuntimeState _runtimeState;
    private readonly string _backOfficePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state used to determine if the application is running.</param>
    /// <param name="basicAuthService">The service providing basic authentication configuration and validation.</param>
    /// <param name="hostingEnvironment">The hosting environment used to resolve the backoffice path.</param>
    public BasicAuthenticationMiddleware(
        IRuntimeState runtimeState,
        IBasicAuthService basicAuthService,
        IHostingEnvironment hostingEnvironment)
    {
        _runtimeState = runtimeState;
        _basicAuthService = basicAuthService;
        _backOfficePath = hostingEnvironment.GetBackOfficePath();
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_runtimeState.Level < RuntimeLevel.Run
            || _basicAuthService.IsBasicAuthEnabled() is false
            || context.Request.IsBackOfficeRequest()
            || context.Request.Path.StartsWithSegments($"{_backOfficePath}/basic-auth")
            || IsAllowedClientRequest(context)
            || _basicAuthService.HasCorrectSharedSecret(context.Request.Headers))
        {
            await next(context);
            return;
        }

        IPAddress? clientIPAddress = context.Connection.RemoteIpAddress;
        if (clientIPAddress is not null && _basicAuthService.IsIpAllowListed(clientIPAddress))
        {
            await next(context);
            return;
        }

        if (await IsAuthenticatedBackOfficeRequestAsync(context))
        {
            await next(context);
            return;
        }

        if (context.TryGetBasicAuthCredentials(out var username, out var password) is false)
        {
            // No authorization header.
            HandleUnauthorized(context);
            return;
        }

        IBackOfficeSignInManager? backOfficeSignInManager =
            context.RequestServices.GetService<IBackOfficeSignInManager>();

        if (backOfficeSignInManager is null || username is null || password is null)
        {
            HandleUnauthorized(context);
            return;
        }

        SignInResult signInResult =
            await backOfficeSignInManager.PasswordSignInAsync(username, password, false, true);

        if (signInResult.Succeeded)
        {
            await next.Invoke(context);
        }
        else if (signInResult.RequiresTwoFactor)
        {
            // Always redirect to the 2FA page, even when RedirectToLoginPage is false.
            // The browser's Basic auth popup cannot complete a 2FA flow.
            var returnPath = WebUtility.UrlEncode(context.Request.GetEncodedPathAndQuery());
            context.Response.Redirect($"{_backOfficePath}/basic-auth/2fa?returnPath={returnPath}", false);
        }
        else
        {
            HandleUnauthorized(context);
        }
    }

    private bool IsAllowedClientRequest(HttpContext context) =>
        context.Request.IsClientSideRequest() && _basicAuthService.IsRedirectToLoginPageEnabled();

    /// <summary>
    /// Checks if the request is already authenticated via the backoffice cookie scheme.
    /// Returns false when backoffice auth services are not registered (e.g. AddCore()-only deployments).
    /// </summary>
    private static async Task<bool> IsAuthenticatedBackOfficeRequestAsync(HttpContext context)
    {
        IAuthenticationSchemeProvider? schemeProvider = context.RequestServices.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is null)
        {
            return false;
        }

        AuthenticationScheme? backOfficeScheme = await schemeProvider.GetSchemeAsync(Cms.Core.Constants.Security.BackOfficeAuthenticationType);
        if (backOfficeScheme is null)
        {
            return false;
        }

        AuthenticateResult authenticateResult = await context.AuthenticateBackOfficeAsync();
        return authenticateResult.Succeeded;
    }

    private void HandleUnauthorized(HttpContext context)
    {
        if (_basicAuthService.IsRedirectToLoginPageEnabled())
        {
            var returnPath = WebUtility.UrlEncode(context.Request.GetEncodedPathAndQuery());

            // Always use the standalone server-rendered login page for basic auth.
            // This is purpose-built for the "authenticate and return to the frontend" flow,
            // avoiding the heavier backoffice SPA + OpenIddict token flow.
            context.Response.Redirect($"{_backOfficePath}/basic-auth/login?returnPath={returnPath}", false);
        }
        else
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Umbraco login\"");
        }
    }
}
