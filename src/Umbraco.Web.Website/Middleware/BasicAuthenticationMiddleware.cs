using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            || !_basicAuthService.IsBasicAuthEnabled()
            || context.Request.IsBackOfficeRequest()
            || context.Request.Path.StartsWithSegments($"{_backOfficePath}/basic-auth")
            || AllowedClientRequest(context)
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

        // Check if backoffice auth scheme is registered before attempting cookie authentication.
        // When only AddCore() is used (without AddBackOfficeSignIn() or AddBackOffice()),
        // the UmbracoBackOffice scheme does not exist and AuthenticateBackOfficeAsync() would throw.
        IAuthenticationSchemeProvider schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        AuthenticationScheme? backOfficeScheme = await schemeProvider.GetSchemeAsync(Cms.Core.Constants.Security.BackOfficeAuthenticationType);

        if (backOfficeScheme is not null)
        {
            AuthenticateResult authenticateResult = await context.AuthenticateBackOfficeAsync();
            if (authenticateResult.Succeeded)
            {
                await next(context);
                return;
            }
        }

        if (context.TryGetBasicAuthCredentials(out var username, out var password))
        {
            IBackOfficeSignInManager? backOfficeSignInManager =
                context.RequestServices.GetService<IBackOfficeSignInManager>();

            if (backOfficeSignInManager is not null && username is not null && password is not null)
            {
                SignInResult signInResult =
                    await backOfficeSignInManager.PasswordSignInAsync(username, password, false, true);

                if (signInResult.Succeeded)
                {
                    await next.Invoke(context);
                }
                else
                {
                    HandleUnauthorized(context);
                }
            }
            else
            {
                HandleUnauthorized(context);
            }
        }
        else
        {
            // no authorization header
            HandleUnauthorized(context);
        }
    }

    private bool AllowedClientRequest(HttpContext context)
    {
        return context.Request.IsClientSideRequest() && _basicAuthService.IsRedirectToLoginPageEnabled();
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
