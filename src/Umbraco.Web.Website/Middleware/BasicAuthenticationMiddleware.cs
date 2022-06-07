using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _runtimeState = runtimeState;
        _basicAuthService = basicAuthService;

        _backOfficePath = globalSettings.CurrentValue.GetBackOfficePath(hostingEnvironment);
    }

    [Obsolete("Use Ctor with all methods. This will be removed in Umbraco 12")]
    public BasicAuthenticationMiddleware(
        IRuntimeState runtimeState,
        IBasicAuthService basicAuthService) : this(
        runtimeState,
        basicAuthService,
        StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
        StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>()
    )
    {

    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_runtimeState.Level < RuntimeLevel.Run
            || !_basicAuthService.IsBasicAuthEnabled()
            || context.Request.IsBackOfficeRequest()
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

        AuthenticateResult authenticateResult = await context.AuthenticateBackOfficeAsync();
        if (authenticateResult.Succeeded)
        {
            await next(context);
            return;
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
            context.Response.Redirect($"{_backOfficePath}#/login/false?returnPath={WebUtility.UrlEncode(context.Request.GetEncodedPathAndQuery())}" , false);
        }
        else
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Umbraco login\"");
        }
    }
}
