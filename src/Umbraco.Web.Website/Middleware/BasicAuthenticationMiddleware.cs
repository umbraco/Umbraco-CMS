using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware
{
    /// <summary>
    ///     Provides basic authentication via back-office credentials for public website access if configured for use and the client IP is not allow listed.
    /// </summary>
    public class BasicAuthenticationMiddleware : IMiddleware
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IBasicAuthService _basicAuthService;

        public BasicAuthenticationMiddleware(
            IRuntimeState runtimeState,
            IBasicAuthService basicAuthService)
        {
            _runtimeState = runtimeState;
            _basicAuthService = basicAuthService;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_runtimeState.Level < RuntimeLevel.Run || context.Request.IsBackOfficeRequest() || !_basicAuthService.IsBasicAuthEnabled())
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
                        SetUnauthorizedHeader(context);
                    }
                }
                else
                {
                    SetUnauthorizedHeader(context);
                }
            }
            else
            {
                // no authorization header
                SetUnauthorizedHeader(context);
            }
        }

        private static void SetUnauthorizedHeader(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Umbraco login\"");
        }
    }
}
